using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class IconDragger : MouseManipulator
{
    Controller controller;

    private Vector2 startPos;
    private Vector2 elemStartPosGlobal;
    private Vector2 elemStartPosLocal;

    VisualElement dragArea;
    VisualElement iconContainer;
    VisualElement dropZone;

    bool isActive;
    public IconDragger(VisualElement root, Controller controller)
    {
        this.controller = controller;

        dragArea = root.Q("DragArea");
        dropZone = root.Q("DropBox");

        isActive = false;
    }
    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
    }
    protected void OnMouseDown(MouseDownEvent e)
    {
        iconContainer = target.parent;

        // Mouse başlangıç pozisyonu
        startPos = e.localMousePosition;

        // Her iki hedef başlangıç ​​pozisyonunu da al
        //elemStartPosGlobal = new Vector2(target.worldBound.xMin, target.worldBound.yMin);
        elemStartPosGlobal = target.worldBound.position;
        elemStartPosLocal = target.layout.position;

        // DragArea'nın aktifliği
        dragArea.style.display = DisplayStyle.Flex;
        dragArea.Add(target);

        // Yeniden konumlandırma
        target.style.top = elemStartPosGlobal.y;
        target.style.left = elemStartPosGlobal.x;

        isActive = true;
        target.CaptureMouse();
        e.StopPropagation();
    }
    protected void OnMouseMove(MouseMoveEvent e)
    {
        if (!isActive || !target.HasMouseCapture())
            return;

        Vector2 diff = e.localMousePosition - startPos;

        target.style.top = target.layout.y + diff.y;
        target.style.left = target.layout.x + diff.x;

        e.StopPropagation();
    }
    protected void OnMouseUp(MouseUpEvent e)
    {
        if (!isActive || !target.HasMouseCapture())
            return;

        if (target.worldBound.Overlaps(dropZone.worldBound))
        {
            dropZone.Add(target);

            target.style.top = dropZone.contentRect.center.y - target.layout.height / 2;
            target.style.left = dropZone.contentRect.center.x - target.layout.width / 2;

            Question question = (Question)target.userData;
            bool isCorrect = controller.game.IsAnswerCorrect(question.answer);

            controller.CheckAnswer(question.answer);

            if (!isCorrect)
            {
                dropZone.Remove(target);
                iconContainer.Add(target);

                target.style.top = elemStartPosLocal.y - iconContainer.contentRect.position.y;
                target.style.left = elemStartPosLocal.x - iconContainer.contentRect.position.x;
            }
        }
        else
        {
            iconContainer.Add(target);

            target.style.top = elemStartPosLocal.y - iconContainer.contentRect.position.y;
            target.style.left = elemStartPosLocal.x - iconContainer.contentRect.position.x;
        }

        isActive = false;
        target.ReleaseMouse();
        e.StopPropagation();

        dragArea.style.display = DisplayStyle.None;
    }

}
