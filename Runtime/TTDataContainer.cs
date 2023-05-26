using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rs64.TexTransTool
{
    public class TTDataContainer : ScriptableObject
    {
        [SerializeField] List<Mesh> _DistMeshs = new List<Mesh>();
        [SerializeField] List<Mesh> _GenereatMeshs = new List<Mesh>();

        [SerializeField] List<Material> _DistMaterials = new List<Material>();
        [SerializeField] List<Material> _GenereatMaterials = new List<Material>();

        public List<Mesh> DistMeshs
        {
            set => _DistMeshs = value;
            get => _DistMeshs;
        }
        public List<Mesh> GenereatMeshs
        {
            set
            {
                if (_GenereatMeshs != null) AssetSaveHelper.DeletSubAssets(_GenereatMeshs);
                _GenereatMeshs = value;
                AssetSaveHelper.SaveSubAssets(this, _GenereatMeshs);
            }
            get => _GenereatMeshs;
        }
        public List<Material> DistMaterials
        {
            set => _DistMaterials = value;
            get => _DistMaterials;
        }
        public List<Material> GenereatMaterials
        {
            set
            {
                if (_GenereatMaterials != null) AssetSaveHelper.DeletSubAssets(_GenereatMaterials);
                _GenereatMaterials = value;
                AssetSaveHelper.SaveSubAssets(this, _GenereatMaterials);
            }
            get => _GenereatMaterials;
        }





    }
}