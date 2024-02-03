﻿//Autogenerated class by script: Assets/Editor/OpenSceneReloadMenu.cs
using UnityEngine;
using UnityEditor;

public static class OpenSceneMenu {

	[MenuItem("Open scene/Lobby", false, 0)]
	static void OpenLobby() {
		OpenScene("Assets/CoreLobby/Scenes/Lobby.unity");
	}

	[MenuItem("Open scene/GameScene", false, 0)]
	static void OpenGameScene() {
		OpenScene("Assets/CoreLobby/Scenes/GameScene.unity");
	}

	[MenuItem("Open scene/GameOver", false, 0)]
	static void OpenGameOver() {
		OpenScene("Assets/CoreLobby/Scenes/GameOver.unity");
	}

	static void OpenScene(string scenePath) {
	if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
			UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
		}
	}
}