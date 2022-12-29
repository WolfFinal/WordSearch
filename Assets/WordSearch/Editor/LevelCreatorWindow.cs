using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

namespace BBG.WordSearch
{
	public class LevelCreatorWindow : EditorWindow
	{
		#region Member Variables

		private TextAsset	wordFile;
		private int			rows;
		private int			cols;
		private int			preferredNumWords;
		private int			maxWordLength;
		private int			numSamplesToGenerate	= 5000;
		private string		randomCharacters		= "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		private int			numLevels;
		private bool		reuseWords;
		private int			reuseAfterNumLevels;

		private string		filenamePrefix;
		private Object		outputFolder;

		private int					numLevelsToGenerate;
		private int					numLevelsGenerated;
		private List<string>		words;
		private List<List<string>>	usedWords;
		private BoardCreatorWorker	boardCreatorWorker;
		private bool				isGenerating;

		#endregion

		#region Properties

		private string WordFileAssetPath
		{
			get { return EditorPrefs.GetString("WordFileAssetPath", ""); }
			set { EditorPrefs.SetString("WordFileAssetPath", value); }
		}

		private string OutputFolderAssetPath
		{
			get { return EditorPrefs.GetString("OutputFolderAssetPath", ""); }
			set { EditorPrefs.SetString("OutputFolderAssetPath", value); }
		}

		private bool IsWorkerRunning	{ get { return boardCreatorWorker != null; } }
		private bool IsWorkerFinished	{ get { return boardCreatorWorker != null && boardCreatorWorker.Stopped; } }

		#endregion

		[MenuItem("Tools/Bizzy Bee Games/Level Creator Window")]
		public static void Init()
		{
			EditorWindow.GetWindow<LevelCreatorWindow>("Level Creator");
		}

		#region Unity Methods

		private void OnEnable()
		{
			if (wordFile == null && !string.IsNullOrEmpty(WordFileAssetPath))
			{
				wordFile = AssetDatabase.LoadAssetAtPath<TextAsset>(WordFileAssetPath);
			}

			if (outputFolder == null && !string.IsNullOrEmpty(OutputFolderAssetPath))
			{
				outputFolder = AssetDatabase.LoadAssetAtPath<Object>(OutputFolderAssetPath);
			}
		}

		private void Update()
		{
			if (isGenerating)
			{
				// If the work is not running then something went wrong so stop generating
				if (!IsWorkerRunning)
				{
					StopGenerating();

					return;
				}

				// Check if the worker has finished generating the current board
				if (IsWorkerFinished)
				{
					OnWorkerFinished();

					// Check if we are done generating levels
					if (numLevelsGenerated >= numLevelsToGenerate)
					{
						StopGenerating();

						return;
					}

					// Start generating the next level
					GenerateNextLevel();
				}
				else
				{
					bool cancelled = EditorUtility.DisplayCancelableProgressBar("Generating Levels", string.Format("Generating level {0} of {1}", (numLevelsGenerated + 1), numLevelsToGenerate), (float)numLevelsGenerated / (float)numLevelsToGenerate);

					if (cancelled)
					{
						Debug.Log("User cancelled generating");

						StopGenerating();
					}
				}
			}
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();

			wordFile 				= EditorGUILayout.ObjectField("Word File", wordFile, typeof(TextAsset), false) as TextAsset;
			rows					= EditorGUILayout.IntField("Rows", rows);
			cols					= EditorGUILayout.IntField("Columns", cols);
			maxWordLength			= EditorGUILayout.IntField("Max Word Length", maxWordLength);
			preferredNumWords		= EditorGUILayout.IntField("Preferred Num Words", preferredNumWords);
			randomCharacters		= EditorGUILayout.TextField("Random Characters", randomCharacters);
			numSamplesToGenerate	= EditorGUILayout.IntField("Num Samples To Generate", numSamplesToGenerate);

			WordFileAssetPath = (wordFile != null) ? AssetDatabase.GetAssetPath(wordFile) : null;

			EditorGUILayout.Space();

			numLevels		= EditorGUILayout.IntField("Num Levels", numLevels);
			reuseWords		= EditorGUILayout.Toggle("Reuse Words", reuseWords);

			GUI.enabled = reuseWords;

			reuseAfterNumLevels = EditorGUILayout.IntField("Reuse After Num Levels", reuseAfterNumLevels);

			GUI.enabled = true;

			EditorGUILayout.Space();

			filenamePrefix	= EditorGUILayout.TextField("Filename Prefix", filenamePrefix);
			outputFolder	= EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(Object), false);

			OutputFolderAssetPath = (outputFolder != null) ? AssetDatabase.GetAssetPath(outputFolder) : null;

			if (outputFolder != null)
			{
				if (!CheckOutputFolder())
				{
					EditorGUILayout.HelpBox("Output Folder must be a folder from the Project window.", MessageType.Error);
				}
			}
			else
			{
				EditorGUILayout.HelpBox("Level files will be placed in the Assets/Resources folder", MessageType.None);
			}

			EditorGUILayout.Space();

			if (wordFile == null)
			{
				EditorGUILayout.HelpBox("Word File is missing", MessageType.Warning);

				GUI.enabled = false;
			}

			if (GUILayout.Button("Generate levels"))
			{
				InitGeneration();
				GenerateNextLevel();
			}

			GUI.enabled = true;
			
			EditorGUILayout.Space();
		}

		#endregion

		#region Private Methods

		private bool CheckOutputFolder()
		{
			string assetPath	= AssetDatabase.GetAssetPath(outputFolder);
			string fullPath		= Application.dataPath + assetPath.Substring("Assets".Length);

			return System.IO.Directory.Exists(fullPath);
		}

		private void InitGeneration()
		{
			numLevelsToGenerate = numLevels;
			numLevelsGenerated	= 0;

			usedWords = new List<List<string>>();

			LoadWordFile();

			isGenerating = true;
		}

		private void LoadWordFile()
		{
			words = new List<string>();

			HashSet<string> addedWords = new HashSet<string>();

			string[] lines = wordFile.text.Split('\n');

			for (int i = 0; i < lines.Length; i++)
			{
				string word = lines[i].Trim().Replace("\r", "");

				if (!string.IsNullOrEmpty(word) && !addedWords.Contains(word) && word.Length <= maxWordLength)
				{
					words.Add(word);
					addedWords.Add(word);
				}
			}
		}

		private void GenerateNextLevel()
		{
			List<string> levelWords = GetNextLevelWords();

			// Check if we can even get the number of words that are suppose to be in the level
			if (levelWords.Count != preferredNumWords)
			{
				Debug.Log("Could not generate the next level because there are only " + levelWords.Count + " valid words to use which is less than the preferred number of words.");

				StopGenerating();

				return;
			}

			BoardCreator.BoardConfig boardConfig = new BoardCreator.BoardConfig();

			boardConfig.rows 						= rows;
			boardConfig.cols 						= cols;
			boardConfig.words						= levelWords;
			boardConfig.randomCharacters			= randomCharacters;
			boardConfig.numSamplesToGenerate		= numSamplesToGenerate;
			boardConfig.algoTimeoutInMilliseconds	= 0;

			// Create the BoardCreatorWorker to actually create the board
			boardCreatorWorker			= new BoardCreatorWorker();
			boardCreatorWorker.Config	= boardConfig;

			boardCreatorWorker.StartWorker();
		}

		private List<string> GetNextLevelWords()
		{
			List<string> levelWords = new List<string>();

			// Add any used words back to the list of words
			if (usedWords.Count > 0 && usedWords.Count >= reuseAfterNumLevels)
			{
				words.AddRange(usedWords[0]);
				usedWords.RemoveAt(0);
			}

			// Pick random words from the words list until we have enough words
			// Randomly choose words to use
			for (int i = 0; i < words.Count && levelWords.Count < preferredNumWords; i++)
			{
				int		randomIndex	= Random.Range(i, words.Count);
				string	randomWord	= words[randomIndex];

				words[randomIndex]	= words[i];
				words[i]			= randomWord;

				levelWords.Add(randomWord);
			}

			return levelWords;
		}

		private void OnWorkerFinished()
		{
			numLevelsGenerated++;

			if (!string.IsNullOrEmpty(boardCreatorWorker.Error))
			{
				Debug.LogError("Failed generating board number " + numLevelsGenerated + ", error: " + boardCreatorWorker.Error);
			}
			else if (boardCreatorWorker.CompletedBoard != null)
			{
				Debug.Log("Num words: " + boardCreatorWorker.CompletedBoard.words.Count);

				usedWords.Add(new List<string>(boardCreatorWorker.CompletedBoard.words));
				words.RemoveAll((string word) => { return boardCreatorWorker.CompletedBoard.words.Contains(word); });

				WriteToLevelFile(boardCreatorWorker.CompletedBoard.ToJson());
			}
			else
			{
				Debug.LogError("CompletedBoard is null!");
			}
		}

		private void WriteToLevelFile(Dictionary<string, object> levelFileJson)
		{
			// Get the files name
			string prefix			= string.IsNullOrEmpty(filenamePrefix) ? "level" : filenamePrefix;
			string levelFilename	= string.Format("{0}_{1}", prefix, numLevelsGenerated);

			// Get the directory path
			string dirPath = "";

			if (outputFolder != null)
			{
				string assetPath = AssetDatabase.GetAssetPath(outputFolder);

				dirPath = Application.dataPath + assetPath.Substring("Assets".Length);

				if (!System.IO.Directory.Exists(dirPath))
				{
					dirPath = "";
				}
			}

			if (string.IsNullOrEmpty(dirPath))
			{
				dirPath = Application.dataPath + "/Resources";

				if (!System.IO.Directory.Exists(dirPath))
				{
					System.IO.Directory.CreateDirectory(dirPath);
				}
			}

			string levelFilePath	= dirPath + "/" + levelFilename + ".json";
			string contents			= Utilities.ConvertToJsonString(levelFileJson);

			System.IO.File.WriteAllText(levelFilePath, contents);

			AssetDatabase.Refresh();
		}

		private void StopGenerating()
		{
			if (boardCreatorWorker != null)
			{
				boardCreatorWorker.Stop();
				boardCreatorWorker = null;
			}

			EditorUtility.ClearProgressBar();

			isGenerating = false;
		}

		#endregion
	}
}
