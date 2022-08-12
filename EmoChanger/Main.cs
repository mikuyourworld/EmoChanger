using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnhollowerRuntimeLib;
using UnityEngine;
using miHoYoEmotion;

namespace EmoChanger
{
    public class Main : MonoBehaviour
    {
        public Main(IntPtr ptr) : base(ptr)
        {
            _gliderTexIndex = 0;
        }

        public Main() : base(ClassInjector.DerivedConstructorPointer<Main>())
        {
            _gliderTexIndex = 0;
            ClassInjector.DerivedConstructorBody(this);
        }

        #region Properties

        private GameObject _avatarRoot;
        private GameObject _npcRoot;
        private GameObject _monsterRoot;
        private GameObject _activeAvatar;
        private GameObject _activeAvatarBody;
        private GameObject _activeAvatarModelParent;
        private GameObject _prevAvatarModelParent;
        private GameObject _gliderRoot;
        private GameObject _weaponRoot;
        private GameObject _weaponRootParent;
        private GameObject _weaponL;
        private GameObject _weaponLParent;
        private GameObject _weaponR;
        private GameObject _weaponRParent;
        private GameObject _npcAvatarModelParent;
        private GameObject _npcWeaponLRoot;
        private GameObject _npcWeaponRRoot;
        private GameObject _npcWeaponRoot;
        private GameObject _npcBodyParent;
        private List<GameObject> _bodyParts = new List<GameObject>();
        private List<GameObject> _npcBodyParts = new List<GameObject>();
        private List<string> _searchResults = new List<string>();
        private List<GameObject> _npcContainer = new List<GameObject>();
        private string _avatarSearch;
        private string _emo1;
        private string _emo2;
        private string _ac1;
        private string _npcType;
        private string[] _files;
        private string _filePath = Path.Combine(Application.dataPath, "tex_test");
        private byte[] _fileData;
        private Texture2D _tex;
        private Animator _activeAvatarAnimator;
        private bool _showMainPanel;
        private bool _showAvatarPanel;
        private bool _showGliderPanel;
        private int _avatarTexIndex;
        private int _gliderTexIndex;
        private Vector3 _npcOffset;

        private Rect _mainRect = new Rect(200, 250, 150, 100);
        private Rect _avatarRect = new Rect(370, 250, 200, 100);
        private Rect _gliderRect = new Rect(590, 250, 200, 100);
        private GUILayoutOption[] _buttonSize;

        #endregion

        public void OnGUI()
        {
            if (!_showMainPanel) return;
            _mainRect = GUILayout.Window(4, _mainRect, (GUI.WindowFunction) TexWindow, "Emo Changer",
                new GUILayoutOption[0]);
        }

        public void TexWindow(int id)
        {
            _buttonSize = new[]
            {
                GUILayout.Width(45),
                GUILayout.Height(20)
            };
            switch (id)
            {
                case 4:
                    {
                        //GUILayout.Label("Texture", new GUILayoutOption[0]);
                        //if (GUILayout.Button("Character Texture", new GUILayoutOption[0]))
                        //    _showAvatarPanel = !_showAvatarPanel;
                        //if (GUILayout.Button("Glider Texture", new GUILayoutOption[0]))
                        //    _showGliderPanel = !_showGliderPanel;
                        //GUILayout.Space(10);
                        //GUILayout.Label("Model", new GUILayoutOption[0]);
                        //GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                        //if (GUILayout.Button("Cut", new GUILayoutOption[0]))
                        //    CutAvatarBody();
                        if (GUILayout.Button("Emotion Changer", new GUILayoutOption[0]))
                            ChangeEmo();
                        //GUILayout.EndHorizontal();

                        GUILayout.Label("Emotion, up - eye, down - mouse.", new GUILayoutOption[0]);
                        _emo1 = GUILayout.TextField(_emo1, new GUILayoutOption[0]);
                        _emo2 = GUILayout.TextField(_emo2, new GUILayoutOption[0]);

                        GUILayout.Label("Pose", new GUILayoutOption[0]);
                        if (GUILayout.Button("Play Pose", new GUILayoutOption[0]))
                            ChangeAction();
                        _ac1 = GUILayout.TextField(_ac1, new GUILayoutOption[0]);
                        if (GUILayout.Button("Reset Pose", new GUILayoutOption[0]))
                            ResetAction();
                        //GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                        //if (GUILayout.Button("Search", new GUILayoutOption[0]))
                        //    SearchObjects();
                        //if (GUILayout.Button("Clear", new GUILayoutOption[0]))
                        //{
                        //    _searchResults.Clear();
                        //    _avatarSearch = "";
                        //}

                        //GUILayout.EndHorizontal();

                        //GUILayout.Space(10);

                        //if (_searchResults.Count > 0)
                        //{
                        //    foreach (var result in _searchResults)
                        //    {
                        //        if (!GUILayout.Button($"{result}", new GUILayoutOption[0])) continue;
                        //        //NpcAvatarChanger(result.gameObject);
                        //    }
                        //}

                        break;
                    }
            }
            GUI.DragWindow();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
                _showMainPanel = !_showMainPanel;

            //if (_showMainPanel)
            //{
            //    Focused = false;
            //    if (_activeAvatarAnimator)
            //        _activeAvatarAnimator.isAnimationPaused = true;
            //}
            //else
            //{
            //    if (_activeAvatarAnimator)
            //        _activeAvatarAnimator.isAnimationPaused = false;
            //}


            if (_avatarRoot == null)
                _avatarRoot = GameObject.Find("/EntityRoot/AvatarRoot");
            if (_npcRoot == null)
                _npcRoot = GameObject.Find("/EntityRoot/NPCRoot");
            if (_monsterRoot == null)
                _monsterRoot = GameObject.Find("/EntityRoot/MonsterRoot");
            if (!_avatarRoot) return;
            try
            {
                if (_activeAvatar == null)
                    FindActiveAvatar();
                if (!_activeAvatar.activeInHierarchy)
                    FindActiveAvatar();
            }
            catch
            {
            }

            _searchResults = _searchResults.Where(item => item != null).ToList();
        }

        #region MainFunctions

        private void ChangeEmo()
        {
            FindActiveAvatar();
            var emo =_activeAvatarModelParent.GetComponent<EmoSync>();
            //设置眼部
            emo.SetEmotion(_emo1, 0);
            //设置嘴部
            emo.SetPhoneme(_emo2, 0);
            emo.EmoFinish();
        }

        private void ChangeAction()
        {
            FindActiveAvatar();
            var ac = _activeAvatarModelParent.GetComponent<Animator>();
            //播放动作
            ac.Play(_ac1);
        }

        private void ResetAction()
        {
            FindActiveAvatar();
            var ac = _activeAvatarModelParent.GetComponent<Animator>();
            //播放动作
            ac.Rebind();
        }

        private void SearchObjects()
        {
            _searchResults.Clear();
            string[] objs = new string[] { "Normal", "Anger", "Happy", "Fear", "Sad", "Disgust", "Surprise", "Puzzled", "EyeClosed", "Shy", "Serious", "Naughty", "Wink", "Laugh", "Hope", "Upset", "Tired", "Sweat", "Angry01", "Angry02", "Angry03", "Angry04", "Angry05", "Angry06", "Closed01", "Closed02", "Closed03", "Closed04", "Closed05", "Happy01", "Happy02", "Happy03", "Happy04", "Happy05", "Normal01", "Normal02", "Normal03", "Normal04", "Normal05", "Normal06", "Sad01", "Sad02", "Sad03", "Sad04", "Sad05", "Surprise01", "Surprise02", "Surprise03", "Surprise04", "Surprise05", "Sweat01", "Sweat02", "Sweat03", "Sweat04", "Sweat05", "Tired01", "Tired02", "Tired03", "Tired04", "Tired05", "Default", "Angry_01", "Angry_02", "Angry_03", "Angry_04", "Default_01", "Doubt_01", "Doubt_02", "Doubt_03", "Doubt_04", "Gentle_01", "Gentle_02", "Gentle_03", "Gentle_04", "HiClosed_01", "HiClosed_02", "HiClosed_03", "HiClosed_04", "HiClosed_05", "HiClosed_06", "HiClosed_07", "LowClosed_01", "LowClosed_02", "LowClosed_03", "LowClosed_04", "LowClosed_05", "LowClosed_06", "LowClosed_07", "MidClosed_01", "MidClosed_02", "MidClosed_03", "MidClosed_04", "MidClosed_05", "MidClosed_06", "MidClosed_07", "Normal_01", "Normal_02", "Normal_03", "Normal_04", "Normal_05", "Surprise_01", "Surprise_02", "Sweat_01", "Sweat_02", "Sweat_03", "Sweat_04", "P_None", "P_A", "P_O", "P_E", "P_I", "P_U", "P_N", "P_Smile01", "P_Smile02", "P_Smile03", "P_Smile04", "P_Smile05", "P_Smile06", "P_Angry01", "P_Angry02", "P_Angry03", "P_Angry04", "P_Doya01", "P_Doya02", "P_Pero01", "P_Pero02", "P_Neko01", "P_Neko02", "P_Delta01", "P_Delta02", "P_Square01", "P_Line01", "P_TalkNone01", "P_TalkNone02", "P_TalkNone03", "P_TalkSmile01", "P_TalkSmile02", "P_TalkSmile03", "P_TalkAngry01", "P_TalkAngry02", "P_TalkAngry03", "P_TalkNone04", "P_TalkDoya01", "P_TalkSquare01", "P_TalkDelta01", "P_TalkSmile04", "P_Default", "P_Default01", "P_Normal01", "P_Doya03", "P_Angry05", "P_Fury01", "P_A01", "P_I01", "P_U01", "P_E01", "P_O01", "P_N01", "P_TalkNormal121", "P_TalkNormal122", "P_TalkNormal131", "P_TalkNormal132", "P_TalkNormal221", "P_TalkNormal222", "P_TalkNormal231", "P_TalkNormal232", "P_TalkNormal321", "P_TalkNormal322", "P_TalkNormal331", "P_TalkSmile121", "P_TalkSmile122", "P_TalkSmile131", "P_TalkSmile132", "P_TalkSmile221", "P_TalkSmile222", "P_TalkSmile231", "P_TalkSmile232", "P_TalkSmile321", "P_TalkSmile322", "P_TalkSmile331", "P_TalkAngry121", "P_TalkAngry122", "P_TalkAngry131", "P_TalkAngry132", "P_TalkAngry221", "P_TalkAngry222", "P_TalkAngry231", "P_TalkAngry232", "P_TalkAngry321", "P_TalkAngry322", "P_TalkAngry331", "P_Default_01", "P_A_01", "P_I_01", "P_U_01", "P_E_01", "P_O_01", "P_N_01", "P_Angry_01", "P_Angry_02", "P_Angry_03", "P_Angry_04", "P_Angry_05", "P_Doya_01", "P_Doya_02", "P_Doya_03", "P_Fury_01", "P_Neko_01", "P_Neko_02", "P_Normal_01", "P_Smile_01", "P_Smile_02", "P_Smile_03", "P_Smile_04", "P_Smile_05", "P_Talk_Angry_121", "P_Talk_Angry_122", "P_Talk_Angry_131", "P_Talk_Angry_132", "P_Talk_Angry_221", "P_Talk_Angry_222", "P_Talk_Angry_231", "P_Talk_Angry_232", "P_Talk_Angry_321", "P_Talk_Angry_322", "P_Talk_Angry_331", "P_Talk_Normal_121", "P_Talk_Normal_122", "P_Talk_Normal_131", "P_Talk_Normal_132", "P_Talk_Normal_221", "P_Talk_Normal_222", "P_Talk_Normal_231", "P_Talk_Normal_232", "P_Talk_Normal_321", "P_Talk_Normal_322", "P_Talk_Normal_331", "P_Talk_Smile_121", "P_Talk_Smile_122", "P_Talk_Smile_131", "P_Talk_Smile_132", "P_Talk_Smile_221", "P_Talk_Smile_222", "P_Talk_Smile_231", "P_Talk_Smile_232", "P_Talk_Smile_321", "P_Talk_Smile_322", "P_Talk_Smile_331", "P_Pero_01", "P_Pero_02" };
            foreach (var a in objs)
            {
                _searchResults.Add(a);
            }

        }

        private void FindActiveAvatar()
        {
            if (_avatarRoot.transform.childCount == 0) return;
            foreach (var a in _avatarRoot.transform)
            {
                var active = a.Cast<Transform>();
                if (!active.gameObject.activeInHierarchy) continue;
                _activeAvatar = active.gameObject;
                FindBody();
            }
        }

        private void FindBody()
        {
            foreach (var a in _activeAvatar.GetComponentsInChildren<Transform>())
            {
                switch (a.name)
                {
                    case "Body":
                        _activeAvatarBody = a.gameObject;
                        break;
                    case "OffsetDummy":
                        _activeAvatarModelParent = a.GetChild(0).gameObject;
                        Loader.Msg($"{_activeAvatarModelParent.transform.name}");
                        _activeAvatarAnimator = _activeAvatarModelParent.GetComponent<Animator>();
                        break;
                    case "WeaponL":
                        _weaponL = a.gameObject;
                        Loader.Msg($"Found {_weaponL.name}");
                        break;
                    case "WeaponR":
                        _weaponR = a.gameObject;
                        Loader.Msg($"Found {_weaponR.name}");
                        break;
                }

                if (a.name.Contains("WeaponRoot"))
                {
                    _weaponRoot = a.gameObject;
                    Loader.Msg($"Found {_weaponRoot.name}");
                }

                if (a.name.Contains("FlycloakRoot"))
                {
                    _gliderRoot = a.gameObject;
                    Loader.Msg($"Found {_gliderRoot.name}");
                }
            }
        }

        private static bool Focused
        {
            get => Cursor.lockState == CursorLockMode.Locked;
            set
            {
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = value == false;
            }
        }

        #endregion
    }

    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}