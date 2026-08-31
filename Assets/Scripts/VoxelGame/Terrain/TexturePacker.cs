using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace VoxelGame.Terrain
{
    [CreateAssetMenu(fileName = "TexturePacker", menuName = "Scriptable Objects/TexturePacker")]
    public class TexturePacker : ScriptableObject
    {
        [SerializeField]
        private Texture[] _textures;

        [SerializeField]
        private Material[] _materials;
        [SerializeField]
        private string _materialParamName;

#if UNITY_EDITOR
        [ContextMenu("Pack Selected")]
        private void TryPackTextures()
        {
            if (_textures.Length % 3 != 0)
            {
                Debug.LogAssertion($"{nameof(TexturePacker)} requires multiple of three textures!");
                return;
            }

            if (_textures.Any(x => x == null))
            {
                Debug.LogAssertion($"{nameof(TexturePacker)} requires non-null textures!");
                return;
            }

            _w = _textures[0].width;
            _h = _textures[0].height;
            if (_textures.Any(texture => texture.width != _w || texture.height != _h))
            {
                Debug.LogAssertion($"{nameof(TexturePacker)} requires textures of equal size!");
                return;
            }

            Pack();
        }

        private void Pack()
        {
            Texture2DArray textureArray = new(_w, _h, _textures.Length, TextureFormat.ARGB32, false);
            textureArray.wrapMode = TextureWrapMode.Repeat;
            textureArray.filterMode = FilterMode.Point;
            for (int i = 0; i < _textures.Length; ++i)
            {
                Texture texture = _textures[i];
                textureArray.CopyPixels(
                    src: texture,
                    srcElement: 0,
                    srcMip: 0,
                    srcX: 0,
                    srcY: 0,
                    srcWidth: _w,
                    srcHeight: _h,
                    dstElement: i,
                    dstMip: 0,
                    dstX: 0,
                    dstY: 0
                  );
            }
            string folderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
            string path = $"{folderPath}/{name}_Output.asset";
            AssetDatabase.CreateAsset(textureArray, path);

            foreach (Material material in _materials)
            {
                if (material == null)
                {
                    continue;
                }
                material.SetTexture(_materialParamName, textureArray);
            }
        }

        private int _w;
        private int _h;
#endif
    }
}
