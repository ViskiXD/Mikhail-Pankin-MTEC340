using System.Collections;
using UnityEngine;
using TMPro;

public class Flicker : MonoBehaviour
{
    [SerializeField] TMP_Text _uiText;
    [SerializeField] float _flickerRate = 0.5f;
    [SerializeField] bool _startOnAwake = true;
    private bool _inCoroutine = false;
    private Coroutine _flickerCoroutine;
    
    private void Start()
    {
        // Null check to prevent errors
        if (_uiText == null)
        {
            Debug.LogWarning("TMP_Text component not assigned to Flicker script on " + gameObject.name);
            return;
        }
        
        if (_startOnAwake)
        {
            StartFlicker();
        }
    }
    
    public void StartFlicker()
    {
        if (!_inCoroutine && _uiText != null)
        {
            _flickerCoroutine = StartCoroutine(Blink());
        }
    }
    
    public void StopFlicker()
    {
        if (_inCoroutine && _flickerCoroutine != null)
        {
            StopCoroutine(_flickerCoroutine);
            _inCoroutine = false;
            if (_uiText != null)
            {
                _uiText.enabled = true; // Ensure text is visible when stopped
            }
        }
    }

    IEnumerator Blink()
    {
        _inCoroutine = true;
        while (true) // Loop continuously
        {
            if (_uiText != null)
            {
                _uiText.enabled = !_uiText.enabled;
            }
            
            yield return new WaitForSeconds(_flickerRate);
        }
    }
    
    private void OnDestroy()
    {
        StopFlicker(); // Clean up when object is destroyed
    }
}
