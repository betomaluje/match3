using System.Threading.Tasks;
using UnityEngine;

public class CameraShake : MonoBehaviour {
    [SerializeField]
    private float _cameraShakeDuration = 0.25f;

    [SerializeField]
    private float _cameraShakeDecreaseFactor = 3f;

    [SerializeField]
    private float _cameraShakeAmount = .2f;

    [SerializeField]
    private GridManager _gridManager;

    private Camera _mainCamera;

    private void Awake() {
        _mainCamera = Camera.main;
    }

    private void OnEnable() {
        _gridManager.OnTileDestroyed += ShakeCamera;
    }

    private void OnDisable() {
        _gridManager.OnTileDestroyed -= ShakeCamera;
    }

    private async void ShakeCamera() {
        var originalPos = _mainCamera.transform.localPosition;
        var duration = _cameraShakeDuration;
        while (duration > 0) {
            _mainCamera.transform.localPosition = originalPos + Random.insideUnitSphere * _cameraShakeAmount;
            duration -= Time.deltaTime * _cameraShakeDecreaseFactor;
            await Task.Yield();
        }

        _mainCamera.transform.localPosition = originalPos;
    }
}