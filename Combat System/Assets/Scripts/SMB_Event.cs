using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gowtham
{
    public enum SMBTiming { OnEnter, OnExit, OnUpdate, OnEnd }
    public class SMB_Event : StateMachineBehaviour
    {
        [System.Serializable]
        public class SMBEvent
        {
            public bool fired;
            public string eventName;
            public SMBTiming timing;
            public float onUpdateFrame = 1;
        }

        [SerializeField] private int m_totalFrames;
        [SerializeField] private int m_currentFrame;
        [SerializeField] private float m_normalizedTime;
        [SerializeField] private float m_normalizedTimeUncapped;
        [SerializeField] private string m_motionTime = "";
        public List<SMBEvent> Events = new List<SMBEvent> ();

        private bool m_hasParam;
        private Comp_SMBEventCurator m_eventCurator;



        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //base.OnStateEnter(animator, stateInfo, layerIndex);
            m_hasParam = HasParameter(animator, m_motionTime);
            m_eventCurator = animator.GetComponent<Comp_SMBEventCurator>();
            m_totalFrames = GetTotalFrames(animator, layerIndex);

            m_normalizedTimeUncapped = stateInfo.normalizedTime;
            m_normalizedTime = m_hasParam ? animator.GetFloat(m_motionTime) : GetNormalizedTime(stateInfo);
            m_currentFrame = GetCurrentFrame(m_totalFrames, m_normalizedTime);


        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        private bool HasParameter(Animator animator, string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName) || string.IsNullOrWhiteSpace(parameterName))
            {
                return false;
            }

            foreach (var parameter in animator.parameters)
            {
                if (parameter.name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetTotalFrames(Animator animator, int layerIndex)
        {
            AnimatorClipInfo[] _clipInfos = animator.GetNextAnimatorClipInfo(layerIndex);
            if (_clipInfos.Length == 0)
            {
                _clipInfos = animator.GetCurrentAnimatorClipInfo(layerIndex);
            }

            AnimationClip clip = _clipInfos[0].clip;
            return Mathf.RoundToInt(clip.length / clip.frameRate);
        }

        private float GetNormalizedTime(AnimatorStateInfo stateInfo)
        {
            return stateInfo.normalizedTime > 1 ? 1 : stateInfo.normalizedTime;
        }
        
        private int GetCurrentFrame(int totalFrames, float normalizedTime)
        {
            return Mathf.RoundToInt(totalFrames * normalizedTime);
        }
    }

}
