using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2.0f;                     //잔상 효과 지속 시간
    public MovementInput moveScript;                    //캐릭터의 움직임을 제어 하는 스크립트
    public float speedBoost = 6;                        //잔항 효과 사용시 속도 증가량 
    public Animator animator;                           //캐릭터의 애니메이션을 제어하는 컴포넌트
    public float animSpeedBoost = 1.5f;                 //잔상 효과 사용 시 애니메이션 속도 증가량

    [Header("Mesh Releted")]
    public float meshRefreshRate = 1.0f;                //잔상이 생성되는 시간 간격
    public float meshDestoryDelay = 3.0f;               //생성된 잔상ㅇ이 사라지는데 걸리는 시간
    public Transform positionToSpwan;                   //잔상이 생성될 위치

    [Header("Shader Releted")]
    public Material mat;                                //잔상이 적용될 재질
    public string shaderVerRef;                         //셰이더에서 사용할 변수 이름 (Alpha)
    public float shaderVarRate = 0.1f;                  //세이더 효과의 변화 속도
    public float shaderVarRefreshRate = 0.05f;           //셰이더 효과가 업데이트 되는 시간 간격

    private SkinnedMeshRenderer[] skinnedRenderer;  //캐릭터의 3D 모델을 랜더링 하는 컴포넌트들
    private bool isTrailActive;                         //현재 잔상 효과가 활성화 되어 있는지 확인 하는 변수

    private float normalSpeed;                          //원래 이동 속도를 저장하는 변수
    private float normalAnimSpeed;                      //원래 애니메이션 속도를 저장하는 변수

    //재질의 투명도를 서서히 변경하는 코루틴
    IEnumerator AnimatMaterialFloat(Material m, float valueGoal, float rate, float refreshRate)
    {
        float valueToAnimate = m.GetFloat(shaderVerRef);   //알파 값을 가져온다

        //목표 값에 도달 할 때 까지 반복
        while (valueToAnimate > valueGoal)
        {
            valueToAnimate -= rate;
            m.SetFloat(shaderVerRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    IEnumerator ActivateTrail(float timeActivated)
    {
        //이전 내용 변수들 저장
        normalSpeed = moveScript.movementSpeed;
        moveScript.movementSpeed = speedBoost;

        normalAnimSpeed = animator.GetFloat("animSpeed");
        animator.SetFloat("animSpeed",animSpeedBoost);

        while (timeActivated > 0)
        {
            timeActivated -= meshRefreshRate;     //시간 카운터를 한다. 이쪽으로

            if(skinnedRenderer == null)
                skinnedRenderer = positionToSpwan.GetComponentsInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < skinnedRenderer.Length; i++)
            {
                GameObject g0gj = new GameObject();
                g0gj.transform.SetPositionAndRotation(positionToSpwan.position, positionToSpwan.rotation);

                MeshRenderer mr = g0gj.AddComponent<MeshRenderer>();
                MeshFilter mf = g0gj.AddComponent<MeshFilter>();

                Mesh m = new Mesh();
                skinnedRenderer[i].BakeMesh(m);
                mf.mesh = m;
                mr.material = mat;

                //잔상의 페이드 아웃 효과 시작
                StartCoroutine(AnimatMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(g0gj,meshDestoryDelay);  //일정 시간 후 잔상 제거
            }
            //다음 잔상 생성까지 대기
            yield return new WaitForSeconds(meshRefreshRate);
        }

        //원래 속도와 애니메이션 속도로 복구
        moveScript.movementSpeed = normalSpeed;
        animator.SetFloat("animSpeed", normalAnimSpeed);
        isTrailActive = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));   //잔상 효과 코루틴 시작
        }
    }
}
