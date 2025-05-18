using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinSingleEffect : MonoBehaviour
{
    public void Init(int coins, Vector3 position, Transform parent)
    {
        gameObject.SetActive(true);
        TextMeshPro text = transform.GetComponent<TextMeshPro>();
        text.text = "+" + coins * MapConstant.COIN_RATIO;
        transform.SetParent(parent, false);
        transform.position = position;
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
        transform.DOMoveY(transform.position.y + 1f, 0.5f).OnComplete(() => {
            text.DOFade(0, 0.5f).OnComplete(() => {
                transform.DOKill();
                text.DOKill();
                Destroy(gameObject);
            });
            if(GetComponentInChildren<SpriteRenderer>())
            {
                GetComponentInChildren<SpriteRenderer>().DOFade(0, 0.5f);
            }
        });
        //
        GameController.UpdateCoin(coins);
    }
}
