using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class stackedSequencesScript : MonoBehaviour {

	//public stuff
	public KMAudio Audio;
	public KMSelectable[] Buttons;
	public MeshRenderer[] Highlights;
	public List<MeshRenderer> ButtonMesh;
	public KMBombModule Module;

	//functionality
	private bool solved = false;
	private bool menu = true;
	private bool struck = false;
	private int selected;
	private List<int>[] input = new List<int>[] { new List<int> { }, new List<int> { } };
	private List<int>[] answer = new List<int>[] { new List<int> { }, new List<int> { } };

	//logging
	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler Press(int pos)
	{
		return delegate
		{
			if (menu)
			{
				switch (pos)
				{
					case 0:
						if (Check())
						{
							Module.HandlePass();
							solved = true;
                            for (int i = 0; i < 3; i++)
                            {
								ButtonMesh[i].material.color = new Color(0, 1, 0);
                            }
						}
						else
						{
							Debug.LogFormat("[Stacked Sequences #{0}] [{1}] does not match expected input of [{2}] in any cyclic arrangement, strike!", _moduleID, input.Select(x => x.Join("")).Join(", "), answer.Select(x => x.Join("")).Join(", "));
							Module.HandleStrike();
							StartCoroutine(Strike());
						}
						break;
					case 1:
						menu = false;
						selected = 0;
						input[0] = new List<int> { };
						break;
					case 2:
						menu = false;
						selected = 1;
						input[1] = new List<int> { };
						break;
				}
			}
			else
			{
				switch (pos)
				{
					case 0:
						menu = true;
						break;
					case 1:
						input[selected].Add(1);
						break;
					case 2:
						input[selected].Add(0);
						break;
				}
			}
			return false;
		};
	}

	void Awake()
	{
		_moduleID = _moduleIdCounter++;
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].OnInteract += Press(i);
			int x = i;
			Buttons[i].OnHighlight += delegate { Highlights[x].material.color = new Color(.375f, .375f, .375f); };
			Buttons[i].OnHighlightEnded += delegate { Highlights[x].material.color = new Color(.125f, .125f, .125f); };
		}
	}

	void Start () {
		answer = Generate(3, 9);
		StartCoroutine(VisualStuff(0.75f));
	}
	

	private IEnumerator VisualStuff(float deltatime)
	{
		yield return null;
		for (int i = 0; i < 3; i++)
		{
			ButtonMesh.Add(Buttons[i].GetComponent<MeshRenderer>());
		}
		while (!solved)
		{
			int offset = Rnd.Range(0, answer[0].Count() * answer[1].Count());
			for (int i = 0; menu && !solved; i++)
			{
				for (float t = 0; t < deltatime && menu && !solved; t += Time.deltaTime)
				{
					if (struck)
					{
                        for (int j = 0; j < 3; j++)
                        {
							ButtonMesh[j].material.color = new Color(1, 0, 0);
						}
					}
					else
					{
						int c = answer[0][(i + offset) % answer[0].Count()] + answer[1][(i + offset) % answer[1].Count()];
						ButtonMesh[0].material.color = new Color(c / 2f, c / 2f, c / 2f);
						for (int j = 0; j < 2; j++)
						{
							if (input[j].Count() != 0)
							{
								c = input[j][i % input[j].Count()] + input[j][i % input[j].Count()];
								ButtonMesh[j + 1].material.color = new Color(c / 2f, c / 2f, c / 2f);
							}
							else
							{
								ButtonMesh[j + 1].material.color = new Color(.5f, .5f, .5f);
							}
						}
					}
					yield return null;
				}
			}
			while (!menu && !solved)
			{
				for (int i = 0; i < input[selected].Count() && !menu && !solved; i++)
				{ 
					for (float t = 0; t < deltatime && !menu && !solved; t += Time.deltaTime)
					{
						if (struck)
						{
							for (int j = 0; j < 3; j++)
							{
								ButtonMesh[j].material.color = new Color(1, 0, 0);
							}
						}
						else
						{
							ButtonMesh[0].material.color = new Color(input[selected][i], input[selected][i], input[selected][i]);
							ButtonMesh[1].material.color = new Color(1, 1, 1);
							ButtonMesh[2].material.color = new Color(0, 0, 0);
						}
						yield return null;
					}
				}
				for (float t = 0; t < deltatime && !menu && !solved; t += Time.deltaTime)
				{
					if (struck)
					{
						for (int j = 0; j < 3; j++)
						{
							ButtonMesh[j].material.color = new Color(1, 0, 0);
						}
					}
                    else
                    {
						ButtonMesh[0].material.color = new Color(.5f, .5f, .5f);
						ButtonMesh[1].material.color = new Color(1, 1, 1);
						ButtonMesh[2].material.color = new Color(0, 0, 0);
					}
					yield return null;
				}
				yield return null;
			}
		}
	}

	private IEnumerator Strike()
    {
		struck = true;
		yield return new WaitForSeconds(1f);
		struck = false;
    }

	private List<int>[] Generate(int lowerbound, int upperbound)
	{
		int[] a = { 0, 0 }, b = { 0, 0 };
		while (b[0] + b[1] != 1)
		{
			for (int i = 0; i < 2; i++)
			{
				a[i] = Rnd.Range(lowerbound, upperbound + 1);
			}
			for (int i = 0; i < 2; i++)
			{
				b[i] = a[i];
			}
			while (b[0] * b[1] != 0)
			{
				b[1] %= b[0];
				int s = b[0];
				b[0] = b[1];
				b[1] = s;
			}
		}
		List<int>[] l = new List<int>[] { new List<int> { }, new List<int> { } };
		for (int x = 0; x < 2; x++)
		{
			bool shorter = true;
			int t = 0;
			while (shorter && t < 20)
			{
				l[x] = new List<int> { };
				for (int i = 0; i < a[x]; i++)
				{
					l[x].Add(Rnd.Range(0, 2));
				}
				shorter = false;
				for (int i = 1; i < a[x]; i++)
				{
					if (a[x] % i == 0)
					{
						List<int> n = new List<int> { };
						for (int j = 0; j < i; j++)
						{
							n.Add(0);
							for (int k = 0; k < a[x] / i; k++)
							{
								n[j] += l[x][i * k + j];
							}
						}
						if (n.Count(y => y == 0 || y == a[x] / i) == i)
						{
							shorter = true;
						}
					}
				}
				t++;
			}
		}
		Debug.LogFormat("[Stacked Sequences #{0}] Generated answers are {1}.", _moduleID, l.Select(x => x.Join("") + " of length " + x.Count()).Join(" and "));
		return l;
	}

	private bool Check()
	{
		List<bool> good = new List<bool> { };
		for (int x = 0; x < 2; x++)
		{
			good.Add(false);
			for (int y = 0; y < 2; y++)
			{
				if (answer[x].Count() == input[y].Count())
				{
					for (int i = 0; i < answer[x].Count(); i++)
					{
						bool valid = true;
						for (int j = 0; j < answer[x].Count(); j++)
						{
							if (answer[x][(i + j) % answer[x].Count()] != input[y][j])
							{
								valid = false;
							}
						}
						good[x] |= valid;
					}
				}
			}
		}
		return good[0] && good[1];
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} 100120122' to press those LEDs. 0 = down, 1 = up, 2 = right.";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		for (int i = 0; i < command.Length; i++)
		{
			if (!"210".Contains(command[i]))
			{
				yield return "sendtochaterror Invalid command.";
				yield break;
			}
		}
		for (int i = 0; i < command.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if ("210"[j] == command[i])
				{
					Buttons[j].OnInteract();
					yield return null;
				}
			}
		}
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		if (!solved)
		{
			if (!menu)
			{
				Buttons[0].OnInteract();
				yield return true;
			}
            for (int x = 0; x < 2; x++)
            {
				Buttons[x + 1].OnInteract();
                for (int i = 0; i < answer[x].Count(); i++)
                {
					Buttons[2 - answer[x][i]].OnInteract();
					yield return true;
				}
				Buttons[0].OnInteract();
				yield return true;
			}
			Buttons[0].OnInteract();
			yield return true;
		}
	}
}
