using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static Thovex.Utility;

[CustomEditor(typeof(PatternAllowanceTest))]
public class PatternAllowanceTestInspector : OdinEditor
{

}

[ExecuteAlways]
public class PatternAllowanceTest : SerializedMonoBehaviour
{
   
}