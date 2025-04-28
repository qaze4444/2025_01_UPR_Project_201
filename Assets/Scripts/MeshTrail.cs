using System.Collections;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2.0f;                 // 잔상 효과 지속 시간
    public MovementInput moveScript;               // 캐릭터의 움직임을 제어하는 스크립트
    public float speedBosst = 6f;                   // 잔상 효과 사용 시 이동 속도 증가량
    public Animator animator;                      // 캐릭터의 애니메이션을 제어하는 컴포넌트
    public float animSpeedBoost = 1.5f;            // 잔상 효과 사용 시 애니메이션 속도 증가량

    [Header("Mesh Related")]
    public float meshRefreshRate = 1.0f;            // 잔상이 생성되는 시간 간격
    public float meshDestoryDelay = 3.0f;           // 생성된 잔상이 사라지는 데 걸리는 시간
    public Transform positionToSpawn;               // 잔상이 생성될 위치

    [Header("Shader Related")]
    public Material mat;                            // 잔상에 적용될 재질
    public string shaderVerRef;                     // 셰이더에서 사용하는 변수 이름 (예: "_Alpha")
    public float shaderVarRate = 0.1f;              // 셰이더 효과의 변화 속도
    public float shaderVarRefreshRate = 0.05f;      // 셰이더 효과가 업데이트되는 시간 간격

    private SkinnedMeshRenderer[] skinnedRenderer;  // 캐릭터의 3D 모델을 렌더링 하는 컴포넌트들
    private bool isTrailActive;                     // 현재 잔상 효과가 활성화 되어 있는지 확인하는 변수

    private float normalSpeed;                      // 원래 이동 속도를 저장하는 변수
    private float normalAnimSpeed;                  // 원래 애니메이션 속도를 저장하는 변수

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTrailActive) // 스페이스바를 누르고 현재 잔상 효과가 비활성화일 때
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));         // 잔상 효과 코루틴 시작
        }
    }

    // 잔상 효과 발동
    IEnumerator ActivateTrail(float timeActivated)
    {
        // 현재 속도와 애니메이션 속도 저장 및 증가된 값으로 변경
        normalSpeed = moveScript.movementSpeed;
        moveScript.movementSpeed = speedBosst;

        normalAnimSpeed = animator.GetFloat("animSpeed");
        animator.SetFloat("animSpeed", animSpeedBoost);

        while (timeActivated > 0)
        {
            timeActivated -= meshRefreshRate;

            if (skinnedRenderer == null)
                skinnedRenderer = positionToSpawn.GetComponentsInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < skinnedRenderer.Length; i++)
            {
                GameObject gObj = new GameObject();
                gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                Mesh m = new Mesh();
                skinnedRenderer[i].BakeMesh(m);
                mf.mesh = m;
                mr.material = mat;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate)); // 페이드 아웃
                Destroy(gObj, meshDestoryDelay); // 일정 시간 후 오브젝트 제거
            }

            yield return new WaitForSeconds(meshRefreshRate); // 다음 잔상 생성까지 대기
        }

        ResetAfterTrail(); // 잔상 종료 후 상태 복구
    }

    // 잔상 종료 후 상태 복구
    private void ResetAfterTrail()
    {
        moveScript.movementSpeed = normalSpeed;
        animator.SetFloat("animSpeed", normalAnimSpeed);
        isTrailActive = false;
    }

    // 재질의 투명도를 서서히 변경하는 코루틴
    IEnumerator AnimateMaterialFloat(Material m, float valueGoal, float rate, float refreshRate)
    {
        float valueToAnimate = m.GetFloat(shaderVerRef);

        while (valueToAnimate > valueGoal)
        {
            valueToAnimate -= rate;
            m.SetFloat(shaderVerRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}