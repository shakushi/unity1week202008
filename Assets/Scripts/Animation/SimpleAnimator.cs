/* ----------------------------------------------------------------- */
/* origin source http://tsubakit1.hateblo.jp/entry/2017/08/02/235736 */
/* ----------------------------------------------------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Timeline;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class SimpleAnimator : MonoBehaviour
{
    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    AnimationClipPlayable prePlayable, currentPlayable;

    Coroutine coroutinePlayAnimation;

    public List<AnimationClip> clipList;

    void Awake()
    {
        graph = PlayableGraph.Create(name);
        AnimationPlayableOutput.Create(graph, name, GetComponent<Animator>());
    }

    void OnDestroy()
    {
        graph.Destroy();
    }

    void Start()
    {
        mixer = AnimationMixerPlayable.Create(graph, 2, true);

        var output = graph.GetOutput(0);
        output.SetSourcePlayable(mixer);
        if (clipList.Count != 0)
        {
            currentPlayable = AnimationClipPlayable.Create(graph, clipList[0]);
            mixer.ConnectInput(0, currentPlayable, 0);
            mixer.SetInputWeight(0, 1);
            graph.Play();
        }
    }

    /* 追加：clipの再生スピードを変えながら変更 */
    public void CrossFade(string animation, float fadeLength, double playspeed)
    {
        AnimationClip clip = clipList.Find((c) => c.name == animation);
        if (coroutinePlayAnimation != null)
            StopCoroutine(coroutinePlayAnimation);
        coroutinePlayAnimation = StartCoroutine(PlayAnimation(clip, fadeLength, playspeed));
    }

    public void CrossFade(string animation, float fadeLength)
    {
        CrossFade(clipList.Find((c) => c.name == animation), fadeLength);
    }

    public void CrossFade(string animation)
    {
        CrossFade(animation, 0.3f);
    }

    public void CrossFade(AnimationClip clip)
    {
        CrossFade(clip, 0.3f);
    }

    public void CrossFade(AnimationClip clip, float fadeLength)
    {
        if (coroutinePlayAnimation != null)
            StopCoroutine(coroutinePlayAnimation);
        coroutinePlayAnimation = StartCoroutine(PlayAnimation(clip, fadeLength));
    }

    public void Play(string clipName)
    {
        Play(clipList.Find((c) => c.name == clipName));
    }

    public void Play(AnimationClip newAnimation)
    {
        if (currentPlayable.IsValid())
            currentPlayable.Destroy();
        DisconnectPlayables();
        currentPlayable = AnimationClipPlayable.Create(graph, newAnimation);
        mixer.ConnectInput(0, currentPlayable, 0);

        mixer.SetInputWeight(1, 0);
        mixer.SetInputWeight(0, 1);
    }

    void DisconnectPlayables()
    {
        graph.Disconnect(mixer, 0);
        graph.Disconnect(mixer, 1);
        if (prePlayable.IsValid())
            prePlayable.Destroy();
    }

    IEnumerator PlayAnimation(AnimationClip newAnimation, float transitionTime, double? playspeed = 0)
    {
        mixer.SetSpeed<AnimationMixerPlayable>(1);
        DisconnectPlayables();

        /* animationclipのスピードを変更 */
        if (playspeed != 0)
        {
            mixer.SetSpeed<AnimationMixerPlayable>((double)playspeed);
        }

        prePlayable = currentPlayable;
        currentPlayable = AnimationClipPlayable.Create(graph, newAnimation);
        mixer.ConnectInput(1, prePlayable, 0);
        mixer.ConnectInput(0, currentPlayable, 0);

        // 指定時間でアニメーションをブレンド
        float waitTime = Time.timeSinceLevelLoad + transitionTime;
        yield return new WaitWhile(() => {
            var diff = waitTime - Time.timeSinceLevelLoad;
            if (diff <= 0)
            {
                mixer.SetInputWeight(1, 0);
                mixer.SetInputWeight(0, 1);
                return false;
            }
            else
            {
                var rate = Mathf.Clamp01(diff / transitionTime);
                mixer.SetInputWeight(1, rate);
                mixer.SetInputWeight(0, 1 - rate);
                return true;
            }
        });

        coroutinePlayAnimation = null;
    }
}