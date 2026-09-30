using Autodesk.Revit.ApplicationServices;

namespace BimBlaster.Vergas;

public enum AppLanguage { Portuguese, English, Spanish }

public static class LanguageResolver
{
    public static AppLanguage Resolve(LanguageType revitLanguage) => revitLanguage switch
    {
        LanguageType.Brazilian_Portuguese => AppLanguage.Portuguese,
        LanguageType.Spanish => AppLanguage.Spanish,
        LanguageType.English_USA or LanguageType.English_GB => AppLanguage.English,
        _ => AppLanguage.Portuguese
    };
}

public static class LocalizedStrings
{
    private static readonly Dictionary<AppLanguage, Dictionary<string, string>> Strings = new()
    {
        [AppLanguage.Portuguese] = new()
        {
            ["ButtonText"] = "Vergas",
            ["ButtonTooltip"] = "Cria vergas e contra-vergas em portas e janelas",
            ["WindowTitle"] = "Vergas e Contra-vergas",
            ["SelectionLabel"] = "Opções de seleção",
            ["ModeWindow"] = "Janela de seleção (window)",
            ["ModeWindowHint"] = "Arraste um retângulo: somente portas e janelas são selecionadas.",
            ["ModePick"] = "Clicar nos elementos",
            ["ModePickHint"] = "Clique em portas e janelas e finalize com Concluir.",
            ["Cancel"] = "Cancelar",
            ["Run"] = "Selecionar e executar",
            ["PromptWindow"] = "Arraste uma janela de seleção sobre as portas e janelas",
            ["PromptPick"] = "Selecione as portas e janelas e clique em Concluir",
            ["NoElements"] = "Nenhuma porta ou janela foi selecionada.",
            ["Warning"] = "Aviso",
            ["Error"] = "Erro",
            ["FamilyNotFound"] = "O tipo de família 'ARQ_VERGA:VERGA' não foi encontrado no projeto.",
            ["SuccessTitle"] = "Sucesso - Vergas",
            ["SuccessMsg"] = "Processo concluído com sucesso!\n\n• Janelas processadas: {0}\n• Portas processadas: {1}\n• Total de elementos criados: {2}\n• Família utilizada: ARQ_VERGA:VERGA",
            ["CriticalError"] = "Ocorreu um erro crítico:\n{0}",
            ["TransactionName"] = "Criar Vergas e Contravergas",
        },
        [AppLanguage.English] = new()
        {
            ["ButtonText"] = "Vergas",
            ["ButtonTooltip"] = "Creates lintels and sills in doors and windows",
            ["WindowTitle"] = "Lintels and Sills",
            ["SelectionLabel"] = "Selection options",
            ["ModeWindow"] = "Selection window",
            ["ModeWindowHint"] = "Drag a rectangle: only doors and windows are selected.",
            ["ModePick"] = "Click elements",
            ["ModePickHint"] = "Click doors and windows and finish with Finish.",
            ["Cancel"] = "Cancel",
            ["Run"] = "Select and run",
            ["PromptWindow"] = "Drag a selection window over the doors and windows",
            ["PromptPick"] = "Select doors and windows and click Finish",
            ["NoElements"] = "No doors or windows were selected.",
            ["Warning"] = "Warning",
            ["Error"] = "Error",
            ["FamilyNotFound"] = "The family type 'ARQ_VERGA:VERGA' was not found in the project.",
            ["SuccessTitle"] = "Success - Vergas",
            ["SuccessMsg"] = "Process completed successfully!\n\n• Windows processed: {0}\n• Doors processed: {1}\n• Total elements created: {2}\n• Family used: ARQ_VERGA:VERGA",
            ["CriticalError"] = "A critical error occurred:\n{0}",
            ["TransactionName"] = "Create Lintels and Sills",
        },
        [AppLanguage.Spanish] = new()
        {
            ["ButtonText"] = "Vergas",
            ["ButtonTooltip"] = "Crea dinteles y antepechos en puertas y ventanas",
            ["WindowTitle"] = "Dinteles y Antepechos",
            ["SelectionLabel"] = "Opciones de selección",
            ["ModeWindow"] = "Ventana de selección (window)",
            ["ModeWindowHint"] = "Arrastre un rectángulo: solo se seleccionan puertas y ventanas.",
            ["ModePick"] = "Hacer clic en los elementos",
            ["ModePickHint"] = "Haga clic en puertas y ventanas y finalice con Finalizar.",
            ["Cancel"] = "Cancelar",
            ["Run"] = "Seleccionar y ejecutar",
            ["PromptWindow"] = "Arrastre una ventana de selección sobre las puertas y ventanas",
            ["PromptPick"] = "Seleccione puertas y ventanas y haga clic en Finalizar",
            ["NoElements"] = "No se seleccionó ninguna puerta ni ventana.",
            ["Warning"] = "Aviso",
            ["Error"] = "Error",
            ["FamilyNotFound"] = "No se encontró el tipo de familia 'ARQ_VERGA:VERGA' en el proyecto.",
            ["SuccessTitle"] = "Éxito - Vergas",
            ["SuccessMsg"] = "¡Proceso completado con éxito!\n\n• Ventanas procesadas: {0}\n• Puertas procesadas: {1}\n• Total de elementos creados: {2}\n• Familia utilizada: ARQ_VERGA:VERGA",
            ["CriticalError"] = "Ocurrió un error crítico:\n{0}",
            ["TransactionName"] = "Crear Dinteles y Antepechos",
        },
    };

    public static string Get(LanguageType revitLanguage, string key) =>
        Get(LanguageResolver.Resolve(revitLanguage), key);

    public static string Get(AppLanguage language, string key) =>
        Strings[language].GetValueOrDefault(key, Strings[AppLanguage.Portuguese][key]);
}