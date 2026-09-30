using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BimBlaster.Vergas;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class VergasCommand : IExternalCommand
{
    private const string FamilyTypeName = "ARQ_VERGA:VERGA";

    private sealed class DoorWindowFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            var cat = elem.Category;
            if (cat == null) return false;
            var id = cat.Id.Value;
            return id == (long)BuiltInCategory.OST_Windows || id == (long)BuiltInCategory.OST_Doors;
        }

        public bool AllowReference(Reference reference, XYZ position) => false;
    }

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var lang = LanguageResolver.Resolve(commandData.Application.Application.Language);
        string T(string k) => LocalizedStrings.Get(lang, k);

        var dlg = new VergasWindow(lang);
        if (dlg.ShowDialog() != true) return Result.Cancelled;

        // 1. Seleção (somente portas e janelas)
        List<Element> selected;
        try
        {
            var filter = new DoorWindowFilter();
            if (dlg.UseRectangle)
                selected = uiDoc.Selection.PickElementsByRectangle(filter, T("PromptWindow")).ToList();
            else
                selected = uiDoc.Selection.PickObjects(ObjectType.Element, filter, T("PromptPick"))
                    .Select(r => doc.GetElement(r)).ToList();
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            return Result.Cancelled;
        }

        var janelas = selected.Where(e => e.Category?.Id.Value == (long)BuiltInCategory.OST_Windows).ToList();
        var portas = selected.Where(e => e.Category?.Id.Value == (long)BuiltInCategory.OST_Doors).ToList();
        if (janelas.Count == 0 && portas.Count == 0)
        {
            TaskDialog.Show(T("Warning"), T("NoElements"));
            return Result.Cancelled;
        }

        // 2. Tipo ARQ_VERGA:VERGA
        var simbolo = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(s => $"{s.Family.Name}:{s.Name}" == FamilyTypeName);
        if (simbolo == null)
        {
            TaskDialog.Show(T("Error"), T("FamilyNotFound"));
            return Result.Failed;
        }

        // 3. Criação em transação única
        var criados = new List<Element>();
        using var t = new Transaction(doc, T("TransactionName"));
        try
        {
            t.Start();
            if (!simbolo.IsActive)
            {
                simbolo.Activate();
                doc.Regenerate();
            }

            foreach (var j in janelas.OfType<FamilyInstance>()) CriarVergas(doc, simbolo, j, true, criados);
            foreach (var p in portas.OfType<FamilyInstance>()) CriarVergas(doc, simbolo, p, false, criados);

            t.Commit();
        }
        catch (Exception ex)
        {
            if (t.HasStarted() && !t.HasEnded()) t.RollBack();
            TaskDialog.Show(T("Error"), string.Format(T("CriticalError"), ex.Message));
            return Result.Failed;
        }

        TaskDialog.Show(T("SuccessTitle"),
            string.Format(T("SuccessMsg"), janelas.Count, portas.Count, criados.Count));
        return Result.Succeeded;
    }

    private static (double apoio, double comprimentoTotal, double altura) CalcularDimensoes(double larguraM)
    {
        double apoio = larguraM <= 2.40 ? 0.30 : 0.40;
        double altura = larguraM <= 1.50 ? 0.10 : (larguraM <= 2.39 ? 0.15 : 0.20);
        return (apoio, larguraM + 2 * apoio, altura);
    }

    private static double ToMeters(double feet) => UnitUtils.ConvertFromInternalUnits(feet, UnitTypeId.Meters);
    private static double ToFeet(double meters) => UnitUtils.ConvertToInternalUnits(meters, UnitTypeId.Meters);

    private static void CriarVergas(Document doc, FamilySymbol simbolo, FamilyInstance inst, bool isJanela, List<Element> criados)
    {
        try
        {
            if (inst.Host is not Wall parede) return;

            double profundidadeFt = ToFeet(Math.Round(ToMeters(parede.Width), 3));

            XYZ wallDir;
            var wallCurve = (parede.Location as LocationCurve)?.Curve;
            if (wallCurve is Line ln)
                wallDir = ln.Direction;
            else if (wallCurve != null && inst.Location is LocationPoint lp0)
            {
                var param = wallCurve.Project(lp0.Point).Parameter;
                wallDir = wallCurve.ComputeDerivatives(param, false).BasisX.Normalize();
            }
            else return;

            var bbox = inst.get_BoundingBox(null);
            if (bbox == null) return;

            // Largura do vão
            double larguraFt = 0.0;
            var pLarg = inst.get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM);
            if (pLarg == null || !pLarg.HasValue)
                pLarg = (doc.GetElement(inst.GetTypeId()) as ElementType)?.get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM);

            if (pLarg != null && pLarg.HasValue)
                larguraFt = pLarg.AsDouble();
            else
            {
                foreach (var nome in new[] { "Largura", "Width", "Vão Livre", "LARGURA" })
                {
                    var pr = inst.LookupParameter(nome) ?? doc.GetElement(inst.GetTypeId())?.LookupParameter(nome);
                    if (pr != null && pr.HasValue) { larguraFt = pr.AsDouble(); break; }
                }
                if (larguraFt <= 0.0)
                    larguraFt = Math.Abs((bbox.Max - bbox.Min).DotProduct(wallDir));
            }
            if (larguraFt <= 0.0) return;

            var (_, comprimentoM, alturaM) = CalcularDimensoes(ToMeters(larguraFt));
            double comprimentoFt = ToFeet(comprimentoM);
            double alturaFt = ToFeet(alturaM);

            var locPt = (inst.Location as LocationPoint)?.Point ?? (bbox.Min + bbox.Max) / 2.0;

            // Nível
            var levelId = inst.LevelId;
            if (levelId == ElementId.InvalidElementId)
                levelId = inst.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM)?.AsElementId() ?? ElementId.InvalidElementId;
            if (levelId == ElementId.InvalidElementId)
                levelId = parede.LevelId;
            if (doc.GetElement(levelId) is not Level nivel) return;

            var top = Instanciar(doc, simbolo, nivel, locPt, wallDir, bbox.Max.Z, 0.0, comprimentoFt, alturaFt, profundidadeFt);
            if (top != null) criados.Add(top);

            if (isJanela)
            {
                var bot = Instanciar(doc, simbolo, nivel, locPt, wallDir, bbox.Min.Z, -alturaFt, comprimentoFt, alturaFt, profundidadeFt);
                if (bot != null) criados.Add(bot);
            }
        }
        catch
        {
            // Um elemento com falha não interrompe o processamento dos demais
        }
    }

    private static FamilyInstance? Instanciar(Document doc, FamilySymbol simbolo, Level nivel, XYZ locPt, XYZ wallDir,
        double zElev, double offsetZ, double comprimentoFt, double alturaFt, double profundidadeFt)
    {
        double zAlvo = zElev + offsetZ;
        var ponto = new XYZ(locPt.X, locPt.Y, zAlvo);
        FamilyInstance? viga = null;

        var metade = wallDir.Multiply(comprimentoFt / 2.0);
        try
        {
            var linha = Line.CreateBound(ponto - metade, ponto + metade);
            viga = doc.Create.NewFamilyInstance(linha, simbolo, nivel, StructuralType.Beam);
        }
        catch { /* cai para inserção por ponto */ }

        if (viga == null)
        {
            try { viga = doc.Create.NewFamilyInstance(ponto, simbolo, nivel, StructuralType.Beam); }
            catch
            {
                try { viga = doc.Create.NewFamilyInstance(ponto, simbolo, nivel, StructuralType.NonStructural); }
                catch { /* sem instância */ }
            }

            if (viga != null)
            {
                var eixo = Line.CreateBound(ponto, ponto + XYZ.BasisZ);
                double ang = XYZ.BasisX.AngleOnPlaneTo(wallDir, XYZ.BasisZ);
                if (Math.Abs(ang) > 0.001)
                    ElementTransformUtils.RotateElement(doc, viga.Id, eixo, ang);
            }
        }

        if (viga == null) return null;

        try
        {
            (viga.LookupParameter("VERGA_Profundidade") ?? viga.LookupParameter("PROFUNDIDADE"))?.Set(profundidadeFt);
            (viga.LookupParameter("VERGA_Largura") ?? viga.LookupParameter("LARGURA"))?.Set(comprimentoFt);
            (viga.LookupParameter("VERGA_Altura") ?? viga.LookupParameter("ALTURA"))?.Set(alturaFt);

            double offsetReal = zAlvo - nivel.Elevation;
            var pStart = viga.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION);
            var pEnd = viga.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION);
            var pZ = viga.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE);
            if (pStart is { IsReadOnly: false }) pStart.Set(offsetReal);
            if (pEnd is { IsReadOnly: false }) pEnd.Set(offsetReal);
            if (pZ is { IsReadOnly: false }) pZ.Set(0.0);
        }
        catch { /* parâmetros opcionais */ }

        return viga;
    }
}