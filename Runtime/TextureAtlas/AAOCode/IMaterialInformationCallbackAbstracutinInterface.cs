#nullable enable
using System;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AAOCode
{
    // https://github.com/anatawa12/AvatarOptimizer/blob/fa97171f46c7fc12e6d738517b9aee901ad7a440/API-Editor/ShaderInformation.cs#L194-L535
    // MaterialInformationCallback を interface にし、それの実態を差し替えるだけで AAO の ShaderInformation の実装を TTT の ITTShaderTextureUsageInformation に流用可能にできる。

    public interface IMaterialInformationCallbackAbstractionInterface
    {

        /// <summary>
        /// Returns the integer value for the property in the material, or null if the property is not set or not found.
        /// </summary>
        /// <param name="propertyName">The name of the property in the material.</param>
        /// <param name="considerAnimation">Whether to consider the animation of the property. If this is true, this function will never </param>
        /// <returns>The integer value for the property in the material, which is same as <see cref="Material.GetInteger(string)"/>, or null if the property is animated.</returns>

        public int? GetInteger(string propertyName, bool considerAnimation = true);


        /// <summary>
        /// Returns the int value for the property in the material, or null if the property is not set or not found.
        /// </summary>
        /// <param name="propertyName">The name of the property in the material.</param>
        /// <param name="considerAnimation">Whether to consider the animation of the property. If this is true, this function will never </param>
        /// <returns>The int value for the property in the material, which is same as <see cref="Material.GetInt(string)"/>, or null if the property is animated.</returns>

        public int? GetInt(string propertyName, bool considerAnimation = true);


        /// <summary>
        /// Returns the float value for the property in the material, or null if the property is not set or not found.
        /// </summary>
        /// <param name="propertyName">The name of the property in the material.</param>
        /// <param name="considerAnimation">Whether to consider the animation of the property. If this is true, this function will never </param>
        /// <returns>The float value for the property in the material, which is same as <see cref="Material.GetFloat(string)"/>, or null if the property is animated.</returns>

        public float? GetFloat(string propertyName, bool considerAnimation = true);


        /// <summary>
        /// Returns the float value for the property in the material, or null if the property is not set or not found.
        /// </summary>
        /// <param name="propertyName">The name of the property in the material.</param>
        /// <param name="considerAnimation">Whether to consider the animation of the property. If this is true, this function will never </param>
        /// <returns>The integer value for the property in the material, which is same as <see cref="Material.GetVector(string)"/>, or null if the property is animated.</returns>

        public Vector4? GetVector(string propertyName, bool considerAnimation = true);


        /// <summary>
        /// Returns if the local Shader Keyword is enabled or not.
        /// </summary>
        /// <param name="keywordName">The name of local shader keyword</param>
        /// <returns>true if the local shader keyword is enabled, false if disabled, null if unknown or mixed.</returns>

        public bool? IsShaderKeywordEnabled(string keywordName);


        /// <summary>
        /// Registers UV Usage that are not considered by Avatar Optimizer.
        ///
        /// This will the UV Channel not affected by optimizations of Avatar Optimizer.
        ///
        /// Avatar Optimizer will preserve the integer part (floor(x, y)) of the UV Value while optimization for each primitive.
        /// If your usage only use integer part of the UV like UV Tile Discard, you should not register the UV Usage as Other UV Usage.
        /// </summary>
        /// <remarks>This API is to provide <see cref="ShaderInformationKind.TextureAndUVUsage"/>.</remarks>
        /// <param name="uvChannel">The UVChannels that are used in the shader.</param>

        public void RegisterOtherUVUsage(UsingUVChannels uvChannel);


        /// <summary>
        /// Registers Texture Usage and UV Usage that are considered by Avatar Optimizer.
        /// 
        /// The texture might go to the atlas / UV Packing if the UsingUVChannels is set and the UV Matrix is known
        /// </summary>
        /// <remarks>This API is to provide <see cref="ShaderInformationKind.TextureAndUVUsage"/>.</remarks>
        /// <param name="textureMaterialPropertyName">The name of the texture property in the material.</param>
        /// <param name="samplerState">The information about the sampler state used for the specified texture.</param>
        /// <param name="uvChannels">The UVChannels that are used in the shader to determine the UV for the texture.</param>
        /// <param name="uvMatrix">The UV Transform Matrix for the texture. This includes textureName_ST scale offset. Null if the UV transfrom is not known.</param>
        /// <remarks>
        /// This section describes the current and planned implementation of UV Packing in the Avatar Optimizer about this function.
        /// 
        /// Currently, Avatar Optimizer does UV Packing if (non-exclusive):
        /// - Texture is reasonably used by small set of materials
        /// - UsingUVChannels is set to only one of UV Channels (per material)
        /// - UV Matrix is known and identity matrix
        /// 
        /// However, Avatar Optimizer will support more complex UV Packing in the future:
        /// - Support UV Matrix with scale is smaller and rotation is multiple of 90 degrees
        /// - multiple UV Channel texture
        /// </remarks>

        public void RegisterTextureUVUsage(
            string textureMaterialPropertyName,
            SamplerStateInformation samplerState,
            UsingUVChannels uvChannels,
            Matrix2x3? uvMatrix);


        /// <summary>
        /// Registers the vertex index usage.
        ///
        /// Calling this will prevent Avatar Optimizer from automatically caning the vertex indices.
        /// If user ask, Avatar Optimizer may change the vertex indices.
        ///
        /// If vertex indices are used only for noise or other purposes that don't affect the mesh much,
        /// tou don't need to call this function.
        /// </summary>
        /// <remarks>This API is to provide <see cref="ShaderInformationKind.VertexIndexUsage"/>.</remarks>

        public void RegisterVertexIndexUsage();
    }


    /// <summary>
    /// The flags to express which UV Channels might be used in the shader.
    ///
    /// Usage of the UV channels might be specified with some other APIs.
    /// </summary>
    [Flags]

    public enum UsingUVChannels
    {
        UV0 = 1,
        UV1 = 2,
        UV2 = 4,
        UV3 = 8,
        UV4 = 16,
        UV5 = 32,
        UV6 = 64,
        UV7 = 128,


        /// <summary>
        /// The UV Channels not from the Mesh UV.
        /// For example, screenspace or color.
        /// </summary>
        NonMesh = 256,
        Unknown = 0x7FFFFFFF,
    }


    /// <summary>
    /// The information about the sampler state for the specified texture.
    ///
    /// You can combine multiple SamplerStateInformation for the texture with `|` operator.
    ///
    /// You can cast string to <c>SamplerStateInformation</c> to use the sampler state for
    /// the specified texture like <c>sampler_MainTex</c> by <c>(SamplerStateInformation)"_MainTex"</c>.
    ///
    /// If your shader is using hardcoded sampler state, you can use the predefined sampler state like
    /// <see cref="SamplerStateInformation.PointClampSampler"/> or <see cref="SamplerStateInformation.LinearRepeatSampler"/>.
    /// </summary>

    public readonly struct SamplerStateInformation
    {
        internal readonly string TextureName;
        internal readonly bool MaterialProperty;



        public SamplerStateInformation(string textureName)
        {
            TextureName = textureName;
            MaterialProperty = true;
        }


        // construct builtin non-material property sampler state
        private SamplerStateInformation(string textureName, bool dummy)
        {
            TextureName = textureName;
            MaterialProperty = false;
        }


        // I don't want to expose equals to public API so I made this internal function instead of overriding Equals
        internal static bool EQ(SamplerStateInformation left, SamplerStateInformation right)
        {
            if (left.MaterialProperty != right.MaterialProperty) return false;
            if (left.TextureName != right.TextureName) return false;
            return true;
        }


        /// <summary>Unknown Sampler. The Avatar Optimizer will never optimize depends on sampler state information</summary>

        public static SamplerStateInformation Unknown { get; } = new("Unknown", false);


        /// <summary>The hard-coded inline Sampler with clamp texture wrapping and point texture filtering mode</summary>

        public static SamplerStateInformation PointClampSampler { get; } = new("PointClamp", false);


        /// <summary>The hard-coded inline Sampler with repeat texture wrapping and point texture filtering mode</summary>

        public static SamplerStateInformation PointRepeatSampler { get; } = new("PointRepeat", false);


        /// <summary>The hard-coded inline Sampler with mirror texture wrapping and point texture filtering mode</summary>

        public static SamplerStateInformation PointMirrorSampler { get; } = new("PointMirror", false);


        /// <summary>The hard-coded inline Sampler with mirror-once texture wrapping and point texture filtering mode</summary>

        public static SamplerStateInformation PointMirrorOnceSampler { get; } =
            new("PointMirrorOnce", false);


        /// <summary>The hard-coded inline Sampler with clamp texture wrapping and linear texture filtering mode</summary>

        public static SamplerStateInformation LinearClampSampler { get; } = new("LinearClamp", false);


        /// <summary>The hard-coded inline Sampler with repeat texture wrapping and linear texture filtering mode</summary>

        public static SamplerStateInformation LinearRepeatSampler { get; } = new("LinearRepeat", false);


        /// <summary>The hard-coded inline Sampler with mirror texture wrapping and linear texture filtering mode</summary>

        public static SamplerStateInformation LinearMirrorSampler { get; } = new("LinearMirror", false);


        /// <summary>The hard-coded inline Sampler with mirror-once texture wrapping and linear texture filtering mode</summary>

        public static SamplerStateInformation LinearMirrorOnceSampler { get; } =
            new("LinearMirrorOnce", false);


        /// <summary>The hard-coded inline Sampler with clamp texture wrapping and anisotropic texture filtering mode</summary>

        public static SamplerStateInformation TrilinearClampSampler { get; } = new("TrilinearClamp", false);


        /// <summary>The hard-coded inline Sampler with repeat texture wrapping and anisotropic texture filtering mode</summary>

        public static SamplerStateInformation TrilinearRepeatSampler { get; } =
            new("TrilinearRepeat", false);


        /// <summary>The hard-coded inline Sampler with mirror texture wrapping and anisotropic texture filtering mode</summary>

        public static SamplerStateInformation TrilinearMirrorSampler { get; } =
            new("TrilinearMirror", false);


        /// <summary>The hard-coded inline Sampler with mirror-once texture wrapping and anisotropic texture filtering mode</summary>

        public static SamplerStateInformation TrilinearMirrorOnceSampler { get; } =
            new("TrilinearMirrorOnce", false);



        public static implicit operator SamplerStateInformation(string textureName) => new(textureName);



        public static SamplerStateInformation operator |(SamplerStateInformation left, SamplerStateInformation right) =>
            Combine(left, right);


        private static SamplerStateInformation Combine(SamplerStateInformation left, SamplerStateInformation right)
        {
            // we may implement better logic in the future
            if (EQ(left, right)) return left;
            return Unknown;
        }
    }



    public readonly struct Matrix2x3 : IEquatable<Matrix2x3>
    {
        private readonly float m00;
        private readonly float m01;
        private readonly float m02;
        private readonly float m10;
        private readonly float m11;
        private readonly float m12;


        public float M00 => m00;
        public float M01 => m01;
        public float M02 => m02;
        public float M10 => m10;
        public float M11 => m11;
        public float M12 => m12;


        public static Matrix2x3 Identity { get; } = new(1, 0, 0, 0, 1, 0);



        public Matrix2x3(float m00, float m01, float m02, float m10, float m11, float m12)
        {
            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
        }



        public static Matrix2x3 operator *(Matrix2x3 a, Matrix2x3 b)
        {
            var m00 = a.m00 * b.m00 + a.m01 * b.m10;
            var m01 = a.m00 * b.m01 + a.m01 * b.m11;
            var m02 = a.m00 * b.m02 + a.m01 * b.m12 + a.m02;
            var m10 = a.m10 * b.m00 + a.m11 * b.m10;
            var m11 = a.m10 * b.m01 + a.m11 * b.m11;
            var m12 = a.m10 * b.m02 + a.m11 * b.m12 + a.m12;


            return new Matrix2x3(m00, m01, m02, m10, m11, m12);
        }



        public Vector2 TransformPoint(Vector2 point) =>
            new(m00 * point.x + m01 * point.y + m02, m10 * point.x + m11 * point.y + m12);



        public static Matrix2x3 NewScaleOffset(Vector4 scaleOffset)
        {
            var scaleX = scaleOffset.x;
            var scaleY = scaleOffset.y;
            var offsetX = scaleOffset.z;
            var offsetY = scaleOffset.w;
            return new Matrix2x3(scaleX, 0, offsetX, 0, scaleY, offsetY);
        }



        public static Matrix2x3 Scale(float scaleX, float scaleY) => new(scaleX, 0, 0, 0, scaleY, 0);



        public static Matrix2x3 Scale(Vector2 scale) => Scale(scale.x, scale.y);



        public static Matrix2x3 Translate(float offsetX, float offsetY) => new(1, 0, offsetX, 0, 1, offsetY);



        public static Matrix2x3 Translate(Vector2 offset) => Translate(offset.x, offset.y);



        public static Matrix2x3 Rotate(float angle)
        {
            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);
            return new Matrix2x3(cos, -sin, 0, sin, cos, 0);
        }



        public bool Equals(Matrix2x3 other) =>
            m00.Equals(other.m00) && m01.Equals(other.m01) && m02.Equals(other.m02) &&
            m10.Equals(other.m10) && m11.Equals(other.m11) && m12.Equals(other.m12);



        public override bool Equals(object? obj) => obj is Matrix2x3 other && Equals(other);



        public override int GetHashCode() => HashCode.Combine(m00, m01, m02, m10, m11, m12);



        public static bool operator ==(Matrix2x3 left, Matrix2x3 right) => left.Equals(right);



        public static bool operator !=(Matrix2x3 left, Matrix2x3 right) => !left.Equals(right);



        public override string ToString() =>
            $"Matrix2x3({m00}, {m01}, {m02}, {m10}, {m11}, {m12})";
    }

}
