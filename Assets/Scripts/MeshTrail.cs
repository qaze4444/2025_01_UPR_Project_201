using System.Collections;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2.0f;                 // �ܻ� ȿ�� ���� �ð�
    public MovementInput moveScript;               // ĳ������ �������� �����ϴ� ��ũ��Ʈ
    public float speedBosst = 6f;                   // �ܻ� ȿ�� ��� �� �̵� �ӵ� ������
    public Animator animator;                      // ĳ������ �ִϸ��̼��� �����ϴ� ������Ʈ
    public float animSpeedBoost = 1.5f;            // �ܻ� ȿ�� ��� �� �ִϸ��̼� �ӵ� ������

    [Header("Mesh Related")]
    public float meshRefreshRate = 1.0f;            // �ܻ��� �����Ǵ� �ð� ����
    public float meshDestoryDelay = 3.0f;           // ������ �ܻ��� ������� �� �ɸ��� �ð�
    public Transform positionToSpawn;               // �ܻ��� ������ ��ġ

    [Header("Shader Related")]
    public Material mat;                            // �ܻ� ����� ����
    public string shaderVerRef;                     // ���̴����� ����ϴ� ���� �̸� (��: "_Alpha")
    public float shaderVarRate = 0.1f;              // ���̴� ȿ���� ��ȭ �ӵ�
    public float shaderVarRefreshRate = 0.05f;      // ���̴� ȿ���� ������Ʈ�Ǵ� �ð� ����

    private SkinnedMeshRenderer[] skinnedRenderer;  // ĳ������ 3D ���� ������ �ϴ� ������Ʈ��
    private bool isTrailActive;                     // ���� �ܻ� ȿ���� Ȱ��ȭ �Ǿ� �ִ��� Ȯ���ϴ� ����

    private float normalSpeed;                      // ���� �̵� �ӵ��� �����ϴ� ����
    private float normalAnimSpeed;                  // ���� �ִϸ��̼� �ӵ��� �����ϴ� ����

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTrailActive) // �����̽��ٸ� ������ ���� �ܻ� ȿ���� ��Ȱ��ȭ�� ��
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));         // �ܻ� ȿ�� �ڷ�ƾ ����
        }
    }

    // �ܻ� ȿ�� �ߵ�
    IEnumerator ActivateTrail(float timeActivated)
    {
        // ���� �ӵ��� �ִϸ��̼� �ӵ� ���� �� ������ ������ ����
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

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate)); // ���̵� �ƿ�
                Destroy(gObj, meshDestoryDelay); // ���� �ð� �� ������Ʈ ����
            }

            yield return new WaitForSeconds(meshRefreshRate); // ���� �ܻ� �������� ���
        }

        ResetAfterTrail(); // �ܻ� ���� �� ���� ����
    }

    // �ܻ� ���� �� ���� ����
    private void ResetAfterTrail()
    {
        moveScript.movementSpeed = normalSpeed;
        animator.SetFloat("animSpeed", normalAnimSpeed);
        isTrailActive = false;
    }

    // ������ ������ ������ �����ϴ� �ڷ�ƾ
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