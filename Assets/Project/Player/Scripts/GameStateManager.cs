using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;

    [Header("Public variables")] [SerializeField]
    private float waitBeforeEndingTime = 6f;


    [SerializeField] float fireworkTime = 5f;
    [SerializeField] int fireworkCount = 50;
    [SerializeField] float _timeToDestroyCastle = 7f;
    [SerializeField] Vector3 localRotateTarget = new Vector3(20f, 0f, 0f);
    [SerializeField] Vector3 localPosTarget = new Vector3(0f, -7f, 0f);


    [Header("References")]
    [SerializeField] private GameObject YouWinPanel;
    [SerializeField] private GameObject YouLosePanel;
    [SerializeField] GameObject _castleRoot;

    public static Action OnGameWin;
    public static Action OnGameLose;


    private void Awake()
    {
        instance = this;
        //OnGameLose += DetonateCastle;
    }
    bool winning = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (winning == false)
            {
                DetonateCastle.DetonateStatic();
            }
        }
            
    }

    public static void LoseGame()
    {
        instance._DestroyCastle();
        EnemyManager.HideEnemies();
        OnGameLose?.Invoke();
    }

    public static void WinGame()
    {
        if (instance == null) return;
        
        instance.StartCoroutine(_LaunchFireworks());
        SoundtrackManager.PlayMenu();
        OnGameWin?.Invoke();
    }  

    static IEnumerator _LaunchFireworks()
    {
        float t = 0f;
        float rate = instance.fireworkTime / instance.fireworkCount;
        float i = 0f;
        while (t <= instance.fireworkTime)
        {
            yield return null;
            if (XRPauseMenu.IsPaused == false)
            { 
                t += Time.deltaTime;
                i += Time.deltaTime;
            }
            if (i >= rate)
            {
                Firework.SpawnFirework();
                i = 0f;
            }
        }
    }

    private void _DestroyCastle()
    {
        var anim = _castleRoot.GetComponent<Animator>();
        anim.Play("Destroy");
    }
    
    public void ReturnToMenu(float t = 0.5f)
    {
        FadeScreen.Fade_Out(t);
        StartCoroutine(_DelayReturnToMenu(t));
    }
    IEnumerator _DelayReturnToMenu(float t)
    {
        yield return new WaitForSeconds(t);
        SceneManager.LoadSceneAsync("MainMenu");
    }
    /*public void _StartEndgame(GameObject panel)
    {
        StartCoroutine(_endgameLogic(panel));
        SoundtrackManager.PlayMenu();
    }
    IEnumerator _endgameLogic(GameObject panel)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(waitBeforeEndingTime);
        ReturnToMenu();
    }*/
    /*public void _DetonateCastle()
    {
        return;
        StartCoroutine(_CastleDetonationRoutine());
        StartCoroutine(_MoveExplodingCastle());
    }
    public List<MeshRenderer> _castleRenderers = new List<MeshRenderer>();
    IEnumerator _CastleDetonationRoutine()
    {
        float timeBetween = _timeToDestroyCastle / (float)_castleRenderers.Count;
        float t = 0f;
        foreach (var renderer in _castleRenderers)
        {
            try { 
            renderer.enabled = false;
            foreach (var col in renderer.GetComponents<Collider>())
                col.enabled = false;
            }
            catch (MissingReferenceException e) { continue; }
            yield return new WaitForSeconds(timeBetween);
            _SpawnExplosionAt(renderer.transform.position);

        }
        //_castleRoot.gameObject.SetActive(false);
    }
    IEnumerator _MoveExplodingCastle()
    {
        yield return new WaitForSeconds(_timeToDestroyCastle / 2f);
        Vector3 startRot = _castleRoot.transform.localEulerAngles;
        Vector3 targetRot = startRot + localRotateTarget;
        Vector3 startPos = _castleRoot.transform.localPosition;
        Vector3 targetPos = startPos + localPosTarget;
        float time = 0f;
        while (time <= _timeToDestroyCastle)
        {
            float t = time / _timeToDestroyCastle;
            _castleRoot.transform.localEulerAngles = Vector3.Slerp(startRot, targetRot, t);
            _castleRoot.transform.localPosition = Vector3.Slerp(startPos, targetPos, t);
            yield return null;
            time += Time.deltaTime;
        }
        //yield break;
    }
    void _SpawnExplosionAt(Vector3 pos)
    {
        GameObject explosion = Instantiate(Resources.Load<GameObject>("Prefabs/explosion"));
        explosion.DestroyAfter(5f);
        explosion.transform.position = pos;
    }*/

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        return;
#endif
        Application.Quit();
    }
}
