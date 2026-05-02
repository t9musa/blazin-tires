using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Countdown : MonoBehaviour
{
    public GameObject CountDown;
    public AudioSource oneTwoThreeSound;
    public AudioSource readySound;

    public GameObject[] usersCars;
    public GameObject[] aiCars;

    public DesertGameManager gameManager;

    public GameObject lapTimer;
    //public PauseMenu pauseMenu;

    void OnEnable()
    {
        // Freeze AI cars until GO
        foreach (var car in aiCars)
        {
            var cc = car.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
            if (cc != null) cc.enabled = false;
            var ai = car.GetComponent<UnityStandardAssets.Vehicles.Car.CarAIControl>();
            if (ai != null) ai.enabled = false;
        }
    }

    void Start()
    {
        oneTwoThreeSound.Play();
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        
        yield return new WaitForSeconds(1f);
        CountDown.GetComponent<TMP_Text>().text = "3";
        CountDown.SetActive(true);

        
        yield return new WaitForSeconds(1f);
        CountDown.SetActive(false);
        CountDown.GetComponent<TMP_Text>().text = "2";
        oneTwoThreeSound.Play();
        CountDown.SetActive(true);
        
        yield return new WaitForSeconds(2f);
        CountDown.SetActive(false);
        CountDown.GetComponent<TMP_Text>().text = "1";
        oneTwoThreeSound.Play();
        CountDown.SetActive(true);
        
        yield return new WaitForSeconds(2f);
        CountDown.SetActive(false);
        CountDown.GetComponent<TMP_Text>().text = "GO";
        readySound.Play();
        CountDown.SetActive(true);

        //enable player input as well as bot input
        for (int i = 0; i < usersCars.Length; i++)
        {
            usersCars[i].GetComponent<PrometeoCarController>().enabled = true;
        }
        for (int i = 0; i < aiCars.Length; i++)
        {
            var cc = aiCars[i].GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
            if (cc != null) cc.enabled = true;
            var ai = aiCars[i].GetComponent<UnityStandardAssets.Vehicles.Car.CarAIControl>();
            if (ai != null) ai.enabled = true;
        }
        gameManager.isRunning = true;

    }
}
