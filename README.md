## Next-Generation-Post-Processing
This is an implementation for "Next Generation Post Processing in Call of Duty Advanced Warfare" in unity URP.Including motionblur、bloom and so on.

### Bloom

![image](https://user-images.githubusercontent.com/56297955/226759304-c5bc7588-0228-4c87-b836-d8805548a61e.png)


Just put the bloom file in your project then you can add the feature.Unlike the traditional bloom algorithm, COD uses a small blur kernel to blur and down sample to a small texture at the same time.So you can use the space exchange for speed, to do bloom use small kernel. 
