using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Async;

namespace Unity.UIElements.Extensions.Tests
{

    public class TestUI : MonoBehaviour
    {
        public UIElementsCollection uiCollection;
        UIDocument doc;
        VisualElement root;

        // Start is called before the first frame update
        void Start()
        {
            doc = GetComponent<UIDocument>();

            UIElementsUtility.Initialize(uiCollection,
                () =>
                {
                    GameObject go = new GameObject();
                    go.transform.parent = transform;
                    var doc = go.AddComponent<UIDocument>();
                    doc.panelSettings = this.doc.panelSettings;
                    return doc;
                });

            root = doc.rootVisualElement;
            if (root != null)
            {
                InitializeView();
            }
        }

        void InitializeView()
        {
            UIElementsUtility.Initialize(root, uiCollection);
            root.Q<Button>("toast").clicked += () =>
            {
                new Toast().Content("message")
                .Show();
            };

            root.Q<Button>("process-bar").clicked += async () =>
            {
                using (new ProgressMask("Progress", "Progress"))
                {
                    await new WaitForSeconds(3);
                }
            };
        }

        // Update is called once per frame
        void Update()
        {
            if (doc.rootVisualElement != null && root != doc.rootVisualElement)
            {
                InitializeView();
            }
        }
    }
}