using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour {

	private const int initialHealth = 100;
	public int maxHealth = initialHealth;
	public int currentHealth = initialHealth;
	public float healthBarLength;
	// Use this for initialization
	void Start () {
	 healthBarLength = Screen.width / 2;
		
	}
	
	// Update is called once per frame
	void Update () {
	 AddjustCurrentHealth(0);
	}
	
	void OnGUI()
	{
		GUI.Box(new Rect(10,40,healthBarLength, 20), currentHealth + "/" + maxHealth);
	}
	
	public void AddjustCurrentHealth(int odj)
	{
		currentHealth += odj;
		
		if (currentHealth < 0)
			currentHealth = 0;
		if (currentHealth > maxHealth)
			currentHealth = maxHealth;
		if (maxHealth < 1)
			maxHealth = 1;
		
		healthBarLength = (Screen.width/2) * (currentHealth / (float)maxHealth);
	}
}
