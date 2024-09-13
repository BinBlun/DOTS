using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AnimationDataListSO : ScriptableObject
{
    public List<AnimationDataSO> animationDataSOList;

    public AnimationDataSO GetAnimationDataSO(AnimationDataSO.AnimationType animationType)
    {
        foreach (AnimationDataSO animationDataSO in animationDataSOList)
        {
            if (animationDataSO.animationType == animationType)
            {
                return animationDataSO;
            }
        }
        Debug.LogError("Could not find animation data SO for animationType: " + animationType);
        return null;
    }
}