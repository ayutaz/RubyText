using System.Collections;
using UnityEngine;

public static class MonoBehaviourExtension
{
    /// <summary>
    /// シングルトンのコルーチンを開始します。以前のコルーチンインスタンスは停止し、処理が重複しないようにします。
    /// </summary>
    /// <param name="self">MonoBehaviour</param>
    /// <param name="co_routine">以前のコルーチンインスタンス</param>
    /// <param name="routine">コルーチン</param>
    public static void StartSingleCoroutine(this MonoBehaviour self, ref Coroutine co_routine, IEnumerator routine)
    {
        if (co_routine != null)
        {
            self.StopCoroutine(co_routine);
        }
        co_routine = self.StartCoroutine(routine);
    }

    /// <summary>
    /// シングルトンのコルーチンを停止します。停止したコルーチンインスタンスは null にします。
    /// </summary>
    /// <param name="self"></param>
    /// <param name="co_routine"></param>
    public static void StopSingleCoroutine(this MonoBehaviour self, ref Coroutine co_routine)
    {
        if (co_routine != null)
        {
            self.StopCoroutine(co_routine);
            co_routine = null;
        }
    }
}
