using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class ConfigureEnemyAnimatorController : Editor
{
    [MenuItem("Tools/Configurar Inimigo")]
    public static void ConfigureEnemyAnimator()
    {
        // Verifica se há um Animator Controller selecionado
        var selectedObject = Selection.activeObject;
        if (selectedObject == null)
        {
            Debug.LogError("Nenhum objeto selecionado. Por favor, selecione um Animator Controller no painel Project.");
            return;
        }

        if (!(selectedObject is AnimatorController))
        {
            Debug.LogError($"O objeto selecionado não é um Animator Controller. Objeto selecionado: {selectedObject.name}");
            return;
        }

        AnimatorController animatorController = selectedObject as AnimatorController;

        // Adiciona os parâmetros Trigger
        AddTriggerParameter(animatorController, "idle");
        AddTriggerParameter(animatorController, "walk");
        AddTriggerParameter(animatorController, "sprint");
        AddTriggerParameter(animatorController, "jumpscare");

        // Configura as transições a partir de Any State
        ConfigureTransitions(animatorController, "Demon|Idle1", "idle");
        ConfigureTransitions(animatorController, "Demon|Walk1", "walk");
        ConfigureTransitions(animatorController, "Demon|Run1", "sprint");
        ConfigureTransitions(animatorController, "Demon|Punch1", "jumpscare");

        Debug.Log("Animator Controller configurado com sucesso!");
    }

    private static void AddTriggerParameter(AnimatorController animatorController, string parameterName)
    {
        if (!animatorController.parameters.Any(p => p.name == parameterName))
        {
            animatorController.AddParameter(parameterName, AnimatorControllerParameterType.Trigger);
        }
    }

    private static void ConfigureTransitions(AnimatorController animatorController, string stateName, string triggerName)
    {
        AnimatorStateMachine rootStateMachine = animatorController.layers[0].stateMachine;

        // Procura o estado na State Machine
        AnimatorState targetState = rootStateMachine.states
            .Select(s => s.state)
            .FirstOrDefault(s => s.name == stateName);

        if (targetState == null)
        {
            Debug.LogWarning($"Estado '{stateName}' não encontrado no Animator Controller.");
            return;
        }

        // Cria a transição de Any State para o estado encontrado
        AnimatorStateTransition transition = rootStateMachine.AddAnyStateTransition(targetState);
        transition.hasExitTime = false;
        transition.duration = 0.1f;
        transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
    }
}