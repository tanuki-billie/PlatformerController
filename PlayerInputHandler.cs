using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 dpadInput = new();
    public float inputBufferTime = 0.2f;
    public InputBufferType bufferedInput = InputBufferType.None;
    private Coroutine bufferCoroutine;
    public event Action OnDrop;
    public event Action OnJumpCanceled;

    public bool jumpHeld = false;
    public InputActionPhase JumpPhase;

    private void BufferInput(InputBufferType input)
    {
        // Don't pull stupid stunts like buffering the same input or buffering no input
        if (input == bufferedInput)
        {
            StopCoroutine(bufferCoroutine);
            bufferCoroutine = StartCoroutine(WaitForBufferedInputToEnd());
            return;
        }
        else if (input == InputBufferType.None) return;

        bufferedInput = input;
        bufferCoroutine = StartCoroutine(WaitForBufferedInputToEnd());
    }

    public void ConsumeBuffer()
    {
        StopCoroutine(bufferCoroutine);
        bufferedInput = InputBufferType.None;
    }

    public void ReadDpad(InputAction.CallbackContext ctx) => dpadInput = ctx.ReadValue<Vector2>().normalized;

    public void ReadJump(InputAction.CallbackContext ctx)
    {
        JumpPhase = ctx.phase;
        if (ctx.started) BufferInput(InputBufferType.Jump);
        else if (ctx.canceled) OnJumpCanceled?.Invoke();
    }

    public void ReadDrop(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnDrop?.Invoke();
    }

    public void ReadAttack(InputAction.CallbackContext ctx)
    {
        if(ctx.started)
        {
            BufferInput(InputBufferType.Attack);
            print("Buffered an attack");
        }
    }

    private IEnumerator WaitForBufferedInputToEnd()
    {
        yield return new WaitForSeconds(inputBufferTime);
        bufferedInput = InputBufferType.None;
    }

#if ENABLE_DEBUG_UI
    private void OnGUI()
    {
        if(bufferedInput != InputBufferType.None)
        {
            GUI.Label(new Rect(0, Screen.height - 20, 200, 20), $"Buffered input: {bufferedInput}");
        }
    }
#endif
}

    public enum InputBufferType
{
    None,
    Attack,
    Jump,
}