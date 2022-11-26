# COMP30019 - Project 1 - Ray Tracer

This is your README.md... you should write anything relevant to your
implementation here.

Please ensure your student details are specified below (*exactly* as on UniMelb
records):

**Name:** Bruce Zhu \
**Student Number:** 1100017 \
**Username:** YICZHU \
**Email:** yiczhu@student.unimelb.edu.au

## Completed stages

Tick the stages bellow that you have completed so we know what to mark (by
editing README.md). **At most 9** marks can be chosen in total for stage
three. If you complete more than this many marks, pick your best one(s) to be
marked!

<!---
Tip: To tick, place an x between the square brackes [ ], like so: [x]
-->

##### Stage 1

- [x] Stage 1.1 - Familiarise yourself with the template
- [x] Stage 1.2 - Implement vector mathematics
- [x] Stage 1.3 - Fire a ray for each pixel
- [x] Stage 1.4 - Calculate ray-entity intersections
- [x] Stage 1.5 - Output primitives as solid colours

##### Stage 2

- [x] Stage 2.1 - Diffuse materials
- [x] Stage 2.2 - Shadow rays 
- [x] Ste 2.3 - Reflective materials
- [x] Stage 2.4 - Refractive materials
- [x] Stage 2.5 - The Fresnel effect
- [x] Stage 2.6 - Anti-aliasing

##### Stage 3

- [ ] Option A - Emissive materials (+6)
- [ ] Option B - Ambient lighting/occlusion (+6)
- [x] Option C - OBJ models (+6)
- [ ] Option D - Glossy materials (+3)
- [ ] Option E - Custom camera orientation (+3)
- [x] Option F - Beer's law (+3)
- [ ] Option G - Depth of field (+3)

*Summary of approaches to stage 3*

The program reads the obj file by iterating through each line and stored the vertices, normals and faces in separate Lists. Vertices and normals were stored as Verctor3 whereas faces were stored as Triangle, so it can simply iterate through each face and perfom ```face.Interset()``` when calling the Intersect method. At the start of implementing the Intersect method, it took around two minutes to render the image. To optimise that, I created a bounding sphere around the object to check if the ray hits the sphere before performing the face hit checking. The idea was that the ray will never hit the object if it doesn't hit the sphere, and it takes much less time to call ```spere.Intersect()``` than interating through all the faces and calling ```face.Intersect()```.

When I started implementing the smooth shading using the vertex normals, it took forever to finish and this was because I first retrived the vertex through ```face.X``` and its index in the vertices List by calling ```vertices.IndexOf(face.X)``` before searching for the normal in the list of normals ```normals[vertices.IndexOf(face.X)]```. As the search is linear in the C# List and each of the list is quite long, it slowed down the rendering. To optimise this, I stored the normal for each vertex of a face (triangle) in separate Lists while reading the file and accessed the normal directly through ```normalsX[faceCount]```, which speeded up the rendering to around 30s. 

For Beer's law, I simply calculated the rate of absorbing and applied to the material colors before multiplying by the accumulative color.



## Final scene render

Be sure to replace ```/images/final_scene.png``` with your final render so it
shows up here.

![My final render](images/final_scene.png)

This render took **11** minutes and **46** seconds on my PC.

I used the following command to render the image exactly as shown:

```
dotnet run -- -f tnet run -- -f tests/final_scene.txt -o images/final_scene.png -x 4
```

## Sample outputs

We have provided you with some sample tests located at ```/tests/*```. So you
have some point of comparison, here are the outputs our ray tracer solution
produces for given command line inputs (for the first two stages, left and right
respectively):

###### Sample 1

```
dotnet run -- -f tests/sample_scene_1.txt -o images/sample_scene_1.png -x 4
```

<p float="left">
  <img src="images/sample_scene_1_s1.png" />
  <img src="images/sample_scene_1_s2.png" /> 
</p>

###### Sample 2

```
dotnet run -- -f tests/sample_scene_2.txt -o images/sample_scene_2.png -x 4
```

<p float="left">
  <img src="images/sample_scene_2_s1.png" />
  <img src="images/sample_scene_2_s2.png" /> 
</p>

## References
[Sctrachpixel](https://www.scratchapixel.com/)

