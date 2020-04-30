# TpacTool
### An unofficial asset explorer for *Mount&Blade II: Bannerlord*

-------------------

#### About

TpacTool is an open source asset explorer which can open TPAC format files, view and export the contents.

TPAC (may be Taleworlds Package) is the asset archive format used by *Mount&Blade II: Bannerlord*. It was introduced at some time during the multiplayer beta, replacing the outdated BRF format of *Warband* and CRF format of early beta, and is still used today.

Taleworlds hasn't released a tool to view or edit the TPAC format yet. Editing asset is necessary for Modding, so here comes TpacTool.

#### Legality

First of all, it must be clear that the copyright of any asset exported through TpacTool belongs to the asset producer. Since the only asset producer is Taleworlds for now, all assets you export with TpacTool are the property of Taleworlds. They should be only used for studying and modding purposes.

#### Requirement

*Minimal:*
- Windows 7
- .Net Framework 4.6.2
- A cpu which is better than fried chip.
- A video card that can draw at least some triangles.

In another word, any computer that can run *Bannerlord* is able to run this software.

#### Installation

You can download the latest build from "Release".

Remember you have to unzip the archive to a directory then run the exe. Don't run it from Winrar or 7Zip directly, it won't startup.

#### How to use

Have you ever used *OpenBRF*, our dear old friend who is the best and only asset managing tool in *Warband*? TpacTool is very similar to it, though TpacTool still lacks some features.

Due to the dependency of TPAC files, in TpacTool you "open" a directory of asset packages instead of a single asset package. After the program startup, click *File* - *Open AssetPackages Folder* from the top-left menu bar, then open the *AssetPackages* directory from the dialog. The *AssetPackages* is in the *Modules\Native* directory of the game. E.g. "*D:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\Native\AssetPackages*"

After loading, the list on the left side of the program window will be filled with the loaded assets. Click one of them to view it. For now, TpacTool **only** supports viewing and exporting of models, materials, textures, skeletons<sup>1</sup> and vertex animations<sup>2</sup>.

1. Include in the rigged model. Individual exporting is unsupported.
2. A.k.a. morph. Include in the model.

#### Screenshot

<img src="https://raw.githubusercontent.com/szszss/TpacTool/master/.github/screenshot1.PNG" alt="screenshot1" />

#### FAQ

**Why are there so many textures missing?**

About half of the textures of *Bannerlord* are stored in the TileSets archives (\*.gtex files). It's a private file format of Graphine, and I can't decode it. Unfortunately, all these textures are important. For example, the diffuse (color) and normal of the material.

I'm still studying the clockwork toy of Graphine. But don't expect it too much, it will cost lots of time, patience, sanity and luck...

**My Photoshop cannot open some exported DDS textures**

The dds plugin of your Photoshop is outdated. Install [the Nvidia DDS Plugin](https://developer.nvidia.com/nvidia-texture-tools-exporter) or [the Intel DDS Plugin](https://software.intel.com/en-us/articles/intel-texture-works-plugin), or try other alternative software, e.g. [Paint.NET](https://www.getpaint.net/).

**When can it import and edit assets?**

Maybe later than packing a new package. The TPAC format is designed for easy loading rather than easy editing. Even modifying a single asset may cause you to recalculate all dependencies of it. I believe that Taleworlds hasn't considered how to edit a TPAC file, too. They repack the entire package everytime after modifying an original asset.

**When can it pack a new package?**

Jeez… could be later. There are still some details I need to know of the TPAC format.

**What is the progress of TpacTool?**

In short: very early work in progress.

Type                   | Resolve<sup>1</sup>| Preview | Export | Edit | Import
-----------------------|---------|---------|--------|------|-------
TPAC Header            |  80%    |
Model                  |  70%    |:heavy_check_mark:|:heavy_check_mark:|:x:|:x:
Material               |  70%    |:heavy_check_mark:|:heavy_check_mark:|:x:|:x:
Texture                |  70%    |:heavy_check_mark:|:heavy_check_mark:|:x:|:x:
Physics Shape<sup>2</sup>|  30%    |:x:|:construction:|:x:|:x:
Skeleton               |  60%    |:x:|:heavy_check_mark:<sup>3</sup>|:x:|:x:
Skeletal Animation     |  80%    |:x:|:construction:|:x:|:x:
Vertex Animation<sup>4</sup>|  90%    |:x:|:heavy_check_mark:<sup>3</sup>|:x:|:x:
Shader                 |  60%    |:x:|:x:|:x:|:x:
Particle<sup>5</sup>   |  30%    |:x:|:x:|:x:|:x:
Procedural Vector Field<sup>6</sup>|  10%    |:x:|:x:|:x:|:x:
Geometry<sup>7</sup>   |   0%    |:x:|:x:|:x:|:x:

1. "Resolve" means learning the actual usage of every single field, not just simply reading it from disk.
1. A.k.a collision body.
1. Include in the model. Individual exporting is unsupported.
1. A.k.a morph. What needs to be clarified is that, the data of vertex animation itself is stored in the mesh of model. What we mentioned before is actually a header or metadata of the animation.
1. Yes... now the particle definitions are stored in the TPAC files.
1. A new toy of *Bannerlord*. Need someone more familiar with the vector fields to explain.
1. Never be actually used. It's the meta information of imported model asset.

**I noticed you split the project to three parts**

Yes, this project is divided into three parts. **TpacTool.Lib** is the basic library for processing the TPAC files. **TpacTool.IO** provides asset import and export with common resource formats. **TpacTool** is the GUI program for most users.

In the near future I will upload these two libraries to Nuget.

**How did you know the format of TPAC**

One night I dreamed of Joe Hill and he told me about it. :triangular_flag_on_post:

#### Documentation

A knowledge base describing the TPAC format will be built.

#### Contribution

This project is open source and licensed under the MIT License. Any contribution is welcome.
