using UnityEditor;


class MyWindow : EditorWindow
{
	[MenuItem("Window/My Window")]
	static void ShowWindow()
	{
        EditorWindow.GetWindow<MyWindow>();
    }

	//void OnGUI()
	//{
	//    // The actual window code goes here
	//}
}
