using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewScrollable : MonoBehaviour
{
    #region Private Serialize Fields
    [SerializeField]
    private bool isHorizontalScroll;
    
    [SerializeField]
    private bool isVerticalScroll;

    [SerializeField]
    private bool isReturn;

    [SerializeField]
    private int scrollStep = -1;

    [SerializeField]
    private GameObject buttonPrefab;

    [SerializeField]
    private Transform contentTransform;

    #endregion

    #region Private Fields
    private float scrollPercentVertical = 0f;
    private float scrollPercentHorizontal = 0f;
    private Vector2 viewportSize;
    private Vector2 contentSize;

    [SerializeField]
    private List<GameObject> test = new List<GameObject>();

    #endregion

    private void Awake()
    {
        // StartCoroutine(WaitUntilSizeNotZero());
        // for (int i = 0; i < 100; i++){
        //     test.Add(Instantiate(buttonPrefab, contentTransform));
        //     test[i].GetComponent<Button>().interactable = false;
        // }
    }

    // Start is called before the first frame update
    void Start()
    {
        // StartCoroutine(RandomButton());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate() {
        viewportSize = GetComponent<ScrollRect>().viewport.rect.size;
        contentSize = GetComponent<ScrollRect>().content.rect.size;

        scrollPercentVertical = buttonPrefab.GetComponent<RectTransform>().rect.height / contentSize.y;
        scrollPercentHorizontal = buttonPrefab.GetComponent<RectTransform>().rect.width / contentSize.x;
    }

    public void ScrollHorizontal(int direction)
    {
        if (this.scrollPercentHorizontal == 0f){
            Debug.LogWarning("[SCROLLABLE WARNING] Couldn't get the scroll percent for horizontal, please try again!");
            return;
        }

        if (!this.isHorizontalScroll){
            return;
        }

        float scrollPerTimeHorizontal = scrollStep == -1 ?
                                      viewportSize.x / contentSize.x :
                                      scrollPercentHorizontal * scrollStep;
        
        if (direction >= 0) {
            this.gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition -= scrollPerTimeHorizontal;

            if (isReturn && this.gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition < 0f) {
                this.gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition = 1f;
            }
        } else {
            this.gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition += scrollPerTimeHorizontal;

            if (isReturn && this.gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition > 1f) {
                this.gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition = 0f;
            }
        }
    }

    public void ScrollVertical(int direction) {
        if (this.scrollPercentVertical == 0f){
            Debug.LogWarning("[SCROLLABLE WARNING] Couldn't get the scroll percent for vertical, please try again!");
            return;
        }
        
        if (!this.isVerticalScroll){
            return;
        }

        float scrollPerTimeVertical = scrollStep == -1 ?
                                      viewportSize.y / contentSize.y :
                                      scrollPercentVertical * scrollStep;
        
        if (direction >= 0) {
            this.gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition += scrollPerTimeVertical;

            if (isReturn && this.gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition > 1f) {
                this.gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
            }
        } else {
            this.gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition -= scrollPerTimeVertical;

            if (isReturn && this.gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition < 0f) {
                this.gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
            }
        }
    }

    IEnumerator WaitUntilSizeNotZero()
    {
        yield return new WaitUntil(
            () => GetComponent<ScrollRect>().viewport.rect.size != Vector2.zero &&
                  GetComponent<ScrollRect>().content.rect.size != Vector2.zero
        );

        viewportSize = GetComponent<ScrollRect>().viewport.rect.size;
        contentSize = GetComponent<ScrollRect>().content.rect.size;

        scrollPercentVertical = buttonPrefab.GetComponent<RectTransform>().rect.height / contentSize.y;
        scrollPercentHorizontal = buttonPrefab.GetComponent<RectTransform>().rect.width / contentSize.x;
    }

    IEnumerator RandomButton() {
        int randomTimes = Random.Range(0, 11);

        for (int i = 0; i < randomTimes; i++){
            int randomIndex = Random.Range(0, test.Count);
            Destroy(test[randomIndex]);
            test.RemoveAt(randomIndex);
        }

        yield return new WaitForSeconds(2);

        randomTimes = Random.Range(0, 11);

        for (int i = 0; i < randomTimes; i++){
            test.Add(Instantiate(buttonPrefab, contentTransform));
        }

        yield return new WaitForSeconds(2);

        StartCoroutine(RandomButton());
    }
}
