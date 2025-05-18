using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using DG.Tweening;

public class ObjectNPC : MonoBehaviour
{
    [Header("Configs")]
    public float chatDuration = 2f;
    public float m_chatScaleTime = 1.5f;

    [HeaderAttribute("References")]
    public AnimationReferenceAsset animIdle;
    public AnimationReferenceAsset[] funnyAnimations;
    public GameObject[] chatBubbles;
    
    [HideInInspector]
    public BoxCollider2D activationRange;
    [HideInInspector]
    public SkeletonAnimation spine;
    [HideInInspector]
    public bool isActived;

    public virtual void Awake() 
    {
        spine = transform.Find("spine").GetComponent<SkeletonAnimation>();    
        activationRange = GetComponent<BoxCollider2D>();
        isActived = false;

        if(chatBubbles.Length > 0)
            foreach(GameObject chat in chatBubbles)
            {
                chat.SetActive(false);
            }
    }
    private void Start() 
    {
        spine.AnimationState.SetAnimation(0, animIdle, true);    
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.tag != GameTag.PLAYER)
            return;
        PlayerIn();
    }

    public virtual void PlayerIn() {}
    public virtual void PlayFunnyAnim()
    {
        if(funnyAnimations.Length == 0)
            return;
        if(spine.AnimationState.GetCurrent(0).Animation != animIdle.Animation)
            return;
        int rand = Random.Range(0, funnyAnimations.Length);
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, funnyAnimations[rand], false);
        trackEntry.Complete += (trackEntry) => spine.AnimationState.SetAnimation(0, animIdle, true);
    }
    public virtual void StartChat()
    {
        if(chatBubbles.Length == 0)
            return;
        StartCoroutine(IChat());
    }
    IEnumerator IChat()
    {
        int totalChat = chatBubbles.Length;
        int currentDialogueIndex = 0;
        while(currentDialogueIndex < totalChat)
        {
            Transform chatTransform = chatBubbles[currentDialogueIndex].transform;
            chatTransform.localScale = Vector3.zero;
            chatBubbles[currentDialogueIndex].SetActive(true);
            chatTransform.DOScale(Vector3.one, m_chatScaleTime);            
            yield return new WaitForSeconds(chatDuration);
            chatBubbles[currentDialogueIndex].SetActive(false);
            currentDialogueIndex++;
        }
        yield return null;
        AllChatComplete();
    }
    public virtual void AllChatComplete()
    {

    }
}
