Shader "Custom/InvisibleMaskNoCulling" {
    // this shader makes an object invisible and obscure objects behind, it also disables back face culling
    // source: https://answers.unity.com/questions/316064/can-i-obscure-an-object-using-an-invisible-object.html
    SubShader {
        // draw after all opaque objects (queue = 2001):
        Tags { "Queue"="Geometry+1" }
        Pass {
            Cull Off // disable back face culling, source: https://forum.unity.com/threads/how-to-turn-off-back-face-culling.329744/#post-3182708
            Blend Zero One // keep the image behind it
        }
    }
}