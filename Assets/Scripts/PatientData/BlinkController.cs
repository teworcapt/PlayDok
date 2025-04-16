using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkController : MonoBehaviour
{
    public Image patientImage;
    public PatientData patientData;

    private float blinkTimer;
    private float nextBlinkTime;

    private void Start()
    {
        nextBlinkTime = Random.Range(0.5f, 3f);
    }

    private void Update()
    {
        blinkTimer += Time.deltaTime;

        if (blinkTimer >= nextBlinkTime)
        {
            StartCoroutine(Blink());
            blinkTimer = 0f;
            nextBlinkTime = Random.Range(2f, 5f);
        }
    }

    public void SetPatient(PatientData data)
    {
        patientData = data;
        if (patientImage != null && patientData != null)
        {
            patientImage.sprite = patientData.selectedSpriteSet.normalSprite;
        }
    }


    private IEnumerator Blink()
    {
        if (patientImage != null && patientData != null)
        {
            patientImage.sprite = patientData.selectedSpriteSet.blinkSprite;
            yield return new WaitForSeconds(0.1f);
            patientImage.sprite = patientData.selectedSpriteSet.normalSprite;
        }
    }
}
