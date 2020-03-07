using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class LoomTest : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Loom.RunAsync(
            ()=> {

            print("This is runed in sub thread."+ Thread.CurrentThread.GetHashCode());

            Loom.RunOnMainThread((a) =>
            {
                print("This is runed in main thread."+ Thread.CurrentThread.GetHashCode());

            }, 1 );
        });
	}
	
}
