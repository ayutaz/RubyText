using System.Collections;
using UnityEngine;

public static class MonoBehaviourExtension
{
    /// <summary>
    /// �V���O���g���̃R���[�`�����J�n���܂��B�ȑO�̃R���[�`���C���X�^���X�͒�~���A�������d�����Ȃ��悤�ɂ��܂��B
    /// </summary>
    /// <param name="self">MonoBehaviour</param>
    /// <param name="co_routine">�ȑO�̃R���[�`���C���X�^���X</param>
    /// <param name="routine">�R���[�`��</param>
    public static void StartSingleCoroutine(this MonoBehaviour self, ref Coroutine co_routine, IEnumerator routine)
    {
        if (co_routine != null)
        {
            self.StopCoroutine(co_routine);
        }
        co_routine = self.StartCoroutine(routine);
    }

    /// <summary>
    /// �V���O���g���̃R���[�`�����~���܂��B��~�����R���[�`���C���X�^���X�� null �ɂ��܂��B
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
