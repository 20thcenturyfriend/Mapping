using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScanMessageUI : MonoBehaviour
{
    public Text messageText;

    public void ShowMessage(string message, Color color)
    {
        messageText.text = message;
        messageText.color = color;
        messageText.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateMessage());
    }

    private IEnumerator AnimateMessage()
    {
        float duration = 2.4f;
        float elapsed = 0f;
        Vector3 startPos = messageText.rectTransform.anchoredPosition;
        Vector3 endPos = startPos + new Vector3(0, 70f, 0);

        Color originalColor = messageText.color;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            messageText.rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
            messageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        messageText.gameObject.SetActive(false);
    }
}
