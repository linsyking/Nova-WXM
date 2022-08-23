using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nova
{
    public class LoadingBar : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            //Utils.FindNovaGameController().InputHelper.DisableInput();
        }

        private void OnDestroy()
        {

            //Utils.FindNovaGameController().InputHelper.EnableInput();
        }

        // Update is called once per frame
        void Update()
        {
            if (Utils.FindNovaGameController().AssetLoader.isABLoaded())
            {
                // Loaded, close
                gameObject.SetActive(false);
            }
            Debug.Log("Waiting for AB Loading");
        }
    }

}
