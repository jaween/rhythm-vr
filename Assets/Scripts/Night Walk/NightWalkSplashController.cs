using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class NightWalkSplashController : MonoBehaviour {

	private void Start () {
        StartCoroutine(SplashCoroutine());
	}
	
	private IEnumerator SplashCoroutine()
    {
        yield return new WaitForSeconds(3.5f);
        SceneManager.LoadScene(1);
    }
}
