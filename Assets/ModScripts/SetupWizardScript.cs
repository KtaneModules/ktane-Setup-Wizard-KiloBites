using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class SetupWizardScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;

	public KMSelectable[] mainButtons, keyboardLetters, keyboardNumbers, folderButtons, accountPrompts;
	public KMSelectable backSpace, shift, reset, submit;

	public GameObject window;
	public GameObject[] pages;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved, isActivated, moduleSelected, canTypeUser, canTypePassword, shiftedLetters, canSubmit, canSolve;

	private Folder[] folders =
	{
		new Folder("Downloads", new int[] { 1, 4 }),
		new Folder("Pictures", new int[] { 3, 5 }),
		new Folder("Documents", new int[] { 0, 1 }),
		new Folder("Music", new int[] { 0, 5 }),
		new Folder("Homework", null, 2),
		new Folder("Videos", new int[] { 2, 4 })
	};

	private Folder startingFolder;
	private Folder[] swappedFolders;

	private ObtainUsername username;

	private int currentPos;

	private string usernameInput, passwordInput, passwordAsterisk;

	private int GetLetterIndex(char c) => "QWERTYUIOPASDFGHJKLZXCVBNM".IndexOf(c);
	private int GetNumberIndex(char n) => "1234567890".IndexOf(n);

    void Awake()
    {

		moduleId = moduleIdCounter++;

		foreach (KMSelectable letter in keyboardLetters)
			letter.OnInteract += delegate () { KeyboardLetterPress(letter); return false; };

		foreach (KMSelectable number in keyboardNumbers)
			number.OnInteract += delegate () { KeyboardNumberPress(number); return false; };

		Module.OnActivate += delegate () { StartCoroutine(Startup()); };
		Module.GetComponent<KMSelectable>().OnFocus += delegate { moduleSelected = true; };
		Module.GetComponent<KMSelectable>().OnDefocus += delegate { moduleSelected = false; };

    }

	
	void Start()
    {
		currentPos = Range(0, 6);

		var shufflingFolders = Enumerable.Range(0, 6).Where(x => x != currentPos).ToList().Shuffle().Take(2).ToArray();


    }

	void KeyboardLetterPress(KMSelectable letter)
	{
		if (moduleSolved || !isActivated || !canSubmit && !canTypeUser)
			return;

		if (usernameInput.Length < 10)
		{
            usernameInput += letter.GetComponentInChildren<TextMesh>().text;
        }
			
	}

	void KeyboardNumberPress(KMSelectable number)
	{
		if (moduleSolved || !isActivated || !canSubmit && !canTypePassword)
			return;

		if (passwordInput.Length < 6)
		{
            passwordInput += number.GetComponentInChildren<TextMesh>().text;
			passwordAsterisk += '*';
        }
			
	}

	void ShiftPress()
	{
		if (moduleSolved || !isActivated || !canSubmit)
			return;

		shiftedLetters = !shiftedLetters;

		var letters = "QWERTYYUIOPASDFGHJKLZXCVBNM";

		for (int i = 0; i < 26; i++)
			keyboardLetters[i].GetComponentInChildren<TextMesh>().text = shiftedLetters ? letters[i].ToString().ToUpperInvariant() : letters[i].ToString().ToLowerInvariant();
	}

	void ResetPress()
	{

	}

	void BackSpacePress()
	{

	}

	IEnumerator Startup()
	{
		yield return null;
	}
	
	
	void Update()
    {
		if (moduleSolved || !isActivated)
			return;

		if (moduleSelected && canTypeUser || canTypePassword)
		{
			for (int ltr = 0; ltr < 26; ltr++)
				if (Input.GetKeyDown(((char)('a' + ltr)).ToString()))
				{
					keyboardLetters[GetLetterIndex((char)('A' + ltr))].OnInteract();
					return;
				}

			var validNumPresses = new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };

			for (int num = 0; num < validNumPresses.Length; num++)
				if (Input.GetKeyDown(validNumPresses[num]))
				{
					keyboardNumbers[int.Parse("1234567890"[num].ToString())].OnInteract();
					return;
				}

			if (Input.GetKeyDown(KeyCode.Backspace))
				backSpace.OnInteract();


		}

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand (string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}





