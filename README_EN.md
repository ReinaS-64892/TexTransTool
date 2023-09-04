# TexTransTool

This tool allows non-destructive, intuitive texture application with decals, color modification, and atlasing to reduce VRAM!

## Installation

VPM is recommended for use with VRChatAvatar. [Add-VPM-Link](https://vpm.rs64.net/add-repo)

Otherwise, from [Latest Releases](https://github.com/SASIKI-64892/TexTransTool/releases/latest).


## Setup

- Create a new GameObject directly under the avatar and name it "TexTransTool".
- Add the components [TexTransParentGroup](Manual/EN/TexTransParentGroup.md) and [AvatarDomainDefinition](Manual/EN/AvatarMaterialDomain.md).
- Create a GameObject as a child of the GameObject "TexTransTool" and name it "TTT Features".

Note: Naming of the GameObjects is optional but it helps for clarity.


Now you can add components offered by TexTransTool to the "TTT Features" GameObject.

- If you want to add gradations to decals like stamps, hair, etc. see [SimpleDecal](Manual/EN/SimpleDecal.md)
- To reduce VRAM by combining textures see [AtlasTexture](Manual/EN/AtlasTexture.md)
