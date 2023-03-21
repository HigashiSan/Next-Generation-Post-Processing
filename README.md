## Next-Generation-Post-Processing
This is an implementation for "Next Generation Post Processing in Call of Duty Advanced Warfare" in unity URP.Including motionblur、bloom and so on.

### Bloom

![image](https://user-images.githubusercontent.com/56297955/226759304-c5bc7588-0228-4c87-b836-d8805548a61e.png)

**Extract bright area**
![image](https://user-images.githubusercontent.com/56297955/226760636-6a2855b6-c001-4ca5-baa5-9fd1e6272a04.png)

**Down sample blur**
![image](https://user-images.githubusercontent.com/56297955/226760726-9ae2cc53-bd00-49d5-a925-ea5f3384ea0b.png)


Just put the bloom file in your project then you can add the feature.

Unlike the traditional bloom algorithm, COD uses a small blur kernel to blur and down sample to a small texture at the same time.So you can use the space exchange for speed, to do bloom use small kernel. 
