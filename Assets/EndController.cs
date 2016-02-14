using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndController : MonoBehaviour
{
    public Text resultsText;
    public Text congratulationsText;
    public Text nameText;
    public ScreenFader fader;

	private void Start ()
    {
        int goodTimings = PlayerPrefs.GetInt("good", 0);
        int badTimings = PlayerPrefs.GetInt("bad", 0);
        int missTimings = PlayerPrefs.GetInt("miss", 0);

        // Sets the text
        string results =
            "Good: " + goodTimings + "\n" +
            "Bad: " + badTimings + "\n" +
            "Miss: " + missTimings;
        resultsText.text = results;
        congratulationsText.text = "Thanks for playing!";
        nameText.text = "Made by Jaween Ediriweera\nFebruary 2016";

        bool fadeFromBlack = true;
        float duration = 2.5f;
        fader.Fade(fadeFromBlack, duration);
    }
}
