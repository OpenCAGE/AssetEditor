using CATHODE;
using CATHODE.ShaderTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AlienPAK
{

    public static class MaterialApplier
    {
        public static void ApplyMaterial(GeometryModel3D geometryModel, Materials.Material material)
        {
            if (geometryModel == null || material == null || material.Shader == null)
                return;

            ImageBrush brush = GetDiffuseTextureBrush(material);

            if (brush != null)
            {
                System.Windows.Media.Color tintColor = GetDiffuseTint(material);

                float uvScale = GetDiffuseUvScale(material);
                if (geometryModel.Geometry is MeshGeometry3D meshGeometry && meshGeometry.TextureCoordinates != null)
                {
                    PointCollection scaledUVs = new PointCollection();
                    foreach (System.Windows.Point uv in meshGeometry.TextureCoordinates)
                    {
                        scaledUVs.Add(new System.Windows.Point(uv.X * uvScale, uv.Y * uvScale));
                    }
                    meshGeometry.TextureCoordinates = scaledUVs;

                    brush.TileMode = TileMode.Tile;
                    brush.Viewport = new Rect(0, 0, 1, 1);
                    brush.ViewportUnits = BrushMappingMode.Absolute;
                }

                bool hasAlphaBlending = HasAlphaBlendingEnabled(material.Shader);
                if (!hasAlphaBlending)
                {
                    brush.Opacity = 1.0;
                }

                Material mat = CreateMaterialWithEffects(brush, material, tintColor);
                SetMaterialsWithBackface(geometryModel, mat);
            }
        }

        private static ImageBrush GetDiffuseTextureBrush(Materials.Material material)
        {
            int diffuseMap = -1;
            switch (material.Shader.Ubershader)
            {
                case SHADER_LIST.CA_ENVIRONMENT:
                    diffuseMap = (int)CA_ENVIRONMENT.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_DECAL_ENVIRONMENT:
                    diffuseMap = (int)CA_DECAL_ENVIRONMENT.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_CHARACTER:
                    diffuseMap = (int)CA_CHARACTER.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_SKIN:
                    diffuseMap = (int)CA_SKIN.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_HAIR:
                    diffuseMap = (int)CA_HAIR.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_EYE:
                    diffuseMap = (int)CA_EYE.SAMPLERS.IRIS_MAP;
                    break;
                case SHADER_LIST.CA_SKIN_OCCLUSION:
                    diffuseMap = (int)CA_SKIN_OCCLUSION.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_DECAL:
                    diffuseMap = (int)CA_DECAL.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_FOGPLANE:
                    diffuseMap = (int)CA_FOGPLANE.SAMPLERS.DIFFUSE_MAP_0;
                    break;
                case SHADER_LIST.CA_EFFECT:
                    diffuseMap = (int)CA_EFFECT.SAMPLERS.DIFFUSE_MAP_0;
                    break;
                case SHADER_LIST.CA_LIQUID_ENVIRONMENT:
                    diffuseMap = (int)CA_LIQUID_ENVIRONMENT.SAMPLERS.LIQUIFLOW_DISTORTION_MAP;
                    break;
                case SHADER_LIST.CA_LIQUID_CHARACTER:
                    diffuseMap = (int)CA_LIQUID_CHARACTER.SAMPLERS.LIQUIFLOW_DISTORTION_MAP;
                    break;
                case SHADER_LIST.CA_SKYDOME:
                    diffuseMap = (int)CA_SKYDOME.SAMPLERS.SKYDOME_MAP;
                    break;
                case SHADER_LIST.CA_SURFACE_EFFECTS:
                    diffuseMap = (int)CA_SURFACE_EFFECTS.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_EFFECT_OVERLAY:
                    diffuseMap = (int)CA_EFFECT_OVERLAY.SAMPLERS.TEXTURE_MAP;
                    break;
                case SHADER_LIST.CA_TERRAIN:
                    diffuseMap = (int)CA_TERRAIN.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_PLANET:
                    diffuseMap = (int)CA_PLANET.SAMPLERS.TERRAIN_MAP;
                    break;
                case SHADER_LIST.CA_LIGHTMAP_ENVIRONMENT:
                    diffuseMap = (int)CA_LIGHTMAP_ENVIRONMENT.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_STREAMER:
                    diffuseMap = (int)CA_STREAMER.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_LOW_LOD_CHARACTER:
                    diffuseMap = (int)CA_LOW_LOD_CHARACTER.SAMPLERS.DIFFUSE_MAP;
                    break;
                case SHADER_LIST.CA_CAMERA_MAP:
                    diffuseMap = (int)CA_CAMERA_MAP.SAMPLERS.DIFFUSE_MAP;
                    break;
            }

            if (diffuseMap != -1 && diffuseMap < material.Shader.SamplerRemaps.Count)
            {
                int diffuseMapIndex = material.Shader.SamplerRemaps[diffuseMap];
                if (diffuseMapIndex != 255 && diffuseMapIndex < material.TextureReferences.Count)
                {
                    ImageSource imageSource = material.TextureReferences[diffuseMapIndex]?.Texture?.ToDDS()?.ToBitmap()?.ToImageSource();
                    if (imageSource != null)
                        return new ImageBrush(imageSource);
                }
            }

            return null;
        }

        private static float GetDiffuseUvScale(Materials.Material material)
        {
            int diffuseUvMult = -1;
            switch (material.Shader.Ubershader)
            {
                case SHADER_LIST.CA_ENVIRONMENT:
                    diffuseUvMult = (int)CA_ENVIRONMENT.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_DECAL_ENVIRONMENT:
                    diffuseUvMult = (int)CA_DECAL_ENVIRONMENT.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_CHARACTER:
                    diffuseUvMult = (int)CA_CHARACTER.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_SKIN:
                    diffuseUvMult = (int)CA_SKIN.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_HAIR:
                    diffuseUvMult = (int)CA_HAIR.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_SKIN_OCCLUSION:
                    diffuseUvMult = (int)CA_SKIN_OCCLUSION.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_SURFACE_EFFECTS:
                    diffuseUvMult = (int)CA_SURFACE_EFFECTS.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_TERRAIN:
                    diffuseUvMult = (int)CA_TERRAIN.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_PLANET:
                    diffuseUvMult = (int)CA_PLANET.PARAMETERS.TERRAIN_MAP_UV_SCALE;
                    break;
                case SHADER_LIST.CA_LIGHTMAP_ENVIRONMENT:
                    diffuseUvMult = (int)CA_LIGHTMAP_ENVIRONMENT.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_STREAMER:
                    diffuseUvMult = (int)CA_STREAMER.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
                case SHADER_LIST.CA_LOW_LOD_CHARACTER:
                    diffuseUvMult = (int)CA_LOW_LOD_CHARACTER.PARAMETERS.DIFFUSE_UV_MULT;
                    break;
            }

            if (diffuseUvMult != -1 && diffuseUvMult < material.Shader.PixelShaderParameterRemaps.Count)
            {
                int remappedIndex = material.Shader.PixelShaderParameterRemaps[diffuseUvMult];
                if (remappedIndex != 255 && remappedIndex < material.PixelShaderConstants.Count)
                {
                    return material.PixelShaderConstants[remappedIndex];
                }
            }

            return 1.0f;
        }

        private static System.Windows.Media.Color GetDiffuseTint(Materials.Material material)
        {
            int diffuseTint = -1;
            switch (material.Shader.Ubershader)
            {
                case SHADER_LIST.CA_ENVIRONMENT:
                    diffuseTint = (int)CA_ENVIRONMENT.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_DECAL_ENVIRONMENT:
                    diffuseTint = (int)CA_DECAL_ENVIRONMENT.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_CHARACTER:
                    diffuseTint = (int)CA_CHARACTER.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_SKIN:
                    diffuseTint = (int)CA_SKIN.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_HAIR:
                    diffuseTint = (int)CA_HAIR.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_SKIN_OCCLUSION:
                    diffuseTint = (int)CA_SKIN_OCCLUSION.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_SURFACE_EFFECTS:
                    diffuseTint = (int)CA_SURFACE_EFFECTS.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_TERRAIN:
                    diffuseTint = (int)CA_TERRAIN.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_LIGHTMAP_ENVIRONMENT:
                    diffuseTint = (int)CA_LIGHTMAP_ENVIRONMENT.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_STREAMER:
                    diffuseTint = (int)CA_STREAMER.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_LOW_LOD_CHARACTER:
                    diffuseTint = (int)CA_LOW_LOD_CHARACTER.PARAMETERS.DIFFUSE_TINT;
                    break;
                case SHADER_LIST.CA_EFFECT:
                    diffuseTint = (int)CA_EFFECT.PARAMETERS.COLOUR_TINT;
                    break;
            }

            if (diffuseTint != -1 && diffuseTint < material.Shader.PixelShaderParameterRemaps.Count)
            {
                int remappedIndex = material.Shader.PixelShaderParameterRemaps[diffuseTint];
                if (remappedIndex != 255 && remappedIndex < material.PixelShaderConstants.Count)
                {
                    UberShaderParameterType? parameterType = ShaderUtility.GetParameterType(material.Shader.Ubershader, "DIFFUSE_TINT");
                    if (parameterType == null && material.Shader.Ubershader == SHADER_LIST.CA_EFFECT)
                        parameterType = ShaderUtility.GetParameterType(material.Shader.Ubershader, "COLOUR_TINT");

                    if (parameterType.HasValue)
                    {
                        float r = 0, g = 0, b = 0, a = 1.0f;

                        switch (parameterType.Value)
                        {
                            case UberShaderParameterType.Float3:
                            case UberShaderParameterType.Half3:
                                if (remappedIndex < material.PixelShaderConstants.Count)
                                    r = material.PixelShaderConstants[remappedIndex];
                                if (remappedIndex + 1 < material.PixelShaderConstants.Count)
                                    g = material.PixelShaderConstants[remappedIndex + 1];
                                if (remappedIndex + 2 < material.PixelShaderConstants.Count)
                                    b = material.PixelShaderConstants[remappedIndex + 2];
                                break;
                            case UberShaderParameterType.Float4:
                            case UberShaderParameterType.Half4:
                                if (remappedIndex < material.PixelShaderConstants.Count)
                                    r = material.PixelShaderConstants[remappedIndex];
                                if (remappedIndex + 1 < material.PixelShaderConstants.Count)
                                    g = material.PixelShaderConstants[remappedIndex + 1];
                                if (remappedIndex + 2 < material.PixelShaderConstants.Count)
                                    b = material.PixelShaderConstants[remappedIndex + 2];
                                if (remappedIndex + 3 < material.PixelShaderConstants.Count)
                                    a = material.PixelShaderConstants[remappedIndex + 3];
                                break;
                        }

                        r = Math.Max(0, Math.Min(1, r));
                        g = Math.Max(0, Math.Min(1, g));
                        b = Math.Max(0, Math.Min(1, b));
                        a = Math.Max(0, Math.Min(1, a));

                        return System.Windows.Media.Color.FromArgb(
                            (byte)(a * 255),
                            (byte)(r * 255),
                            (byte)(g * 255),
                            (byte)(b * 255)
                        );
                    }
                }
            }

            return System.Windows.Media.Colors.Transparent;
        }

        private static Material CreateMaterialWithEffects(ImageBrush brush, Materials.Material material, System.Windows.Media.Color diffuseTintColor)
        {
            DiffuseMaterial diffuseMat = new DiffuseMaterial(brush);
            diffuseMat.AmbientColor = System.Windows.Media.Colors.White;

            bool needsTinting = !IsColorTransparentOrWhite(diffuseTintColor);
            System.Windows.Media.Color emissiveTintColor = GetEmissiveTint(material);
            float emissiveMult = GetEmissiveMult(material);
            bool needsEmissive = !IsColorTransparentOrWhite(emissiveTintColor) || emissiveMult > 0.1f;

            if (!needsTinting && !needsEmissive)
                return diffuseMat;

            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.Children.Add(diffuseMat);

            if (needsTinting)
            {
                float tintIntensity = 0.15f;
                System.Windows.Media.Color emissiveTint = System.Windows.Media.Color.FromArgb(
                    (byte)(diffuseTintColor.A * 0.5f),
                    (byte)Math.Min(255, (int)(diffuseTintColor.R * tintIntensity)),
                    (byte)Math.Min(255, (int)(diffuseTintColor.G * tintIntensity)),
                    (byte)Math.Min(255, (int)(diffuseTintColor.B * tintIntensity))
                );

                EmissiveMaterial tintMaterial = new EmissiveMaterial(new SolidColorBrush(emissiveTint));
                materialGroup.Children.Add(tintMaterial);
            }

            if (needsEmissive)
            {
                if (IsColorTransparentOrWhite(emissiveTintColor))
                    emissiveTintColor = System.Windows.Media.Colors.White;

                if (emissiveMult <= 0)
                    emissiveMult = 1.0f;

                emissiveMult = Math.Min(emissiveMult, 3.0f);

                System.Windows.Media.Color finalEmissive = System.Windows.Media.Color.FromArgb(
                    emissiveTintColor.A,
                    (byte)Math.Min(255, (int)(emissiveTintColor.R * emissiveMult * 0.5f)),
                    (byte)Math.Min(255, (int)(emissiveTintColor.G * emissiveMult * 0.5f)),
                    (byte)Math.Min(255, (int)(emissiveTintColor.B * emissiveMult * 0.5f))
                );

                EmissiveMaterial emissiveMaterial = new EmissiveMaterial(new SolidColorBrush(finalEmissive));
                materialGroup.Children.Add(emissiveMaterial);
            }

            return materialGroup;
        }

        private static void SetMaterialsWithBackface(GeometryModel3D geometryModel, Material frontMaterial)
        {
            geometryModel.Material = frontMaterial;

            if (frontMaterial is DiffuseMaterial frontDiffuse)
            {
                DiffuseMaterial backMaterial = new DiffuseMaterial(frontDiffuse.Brush);
                backMaterial.AmbientColor = frontDiffuse.AmbientColor;
                geometryModel.BackMaterial = backMaterial;
            }
            else if (frontMaterial is MaterialGroup frontGroup)
            {
                foreach (Material mat in frontGroup.Children)
                {
                    if (mat is DiffuseMaterial dm)
                    {
                        DiffuseMaterial backMaterial = new DiffuseMaterial(dm.Brush);
                        backMaterial.AmbientColor = dm.AmbientColor;
                        geometryModel.BackMaterial = backMaterial;
                        break;
                    }
                }
            }
        }

        private static bool IsColorTransparentOrWhite(System.Windows.Media.Color color)
        {
            return (color.R == 255 && color.G == 255 && color.B == 255 && color.A == 255) ||
                   (color.A == 0 && color.R == 0 && color.G == 0 && color.B == 0);
        }

        private static float GetEmissiveMult(Materials.Material material)
        {
            int emissiveMult = -1;
            switch (material.Shader.Ubershader)
            {
                case SHADER_LIST.CA_ENVIRONMENT:
                    emissiveMult = (int)CA_ENVIRONMENT.PARAMETERS.EMISSIVE_MULT;
                    break;
                case SHADER_LIST.CA_CHARACTER:
                    emissiveMult = (int)CA_CHARACTER.PARAMETERS.EMISSIVE_MULT;
                    break;
                case SHADER_LIST.CA_LIGHTMAP_ENVIRONMENT:
                    emissiveMult = (int)CA_LIGHTMAP_ENVIRONMENT.PARAMETERS.EMISSIVE_MULT;
                    break;
            }

            if (emissiveMult != -1 && emissiveMult < material.Shader.PixelShaderParameterRemaps.Count)
            {
                int remappedIndex = material.Shader.PixelShaderParameterRemaps[emissiveMult];
                if (remappedIndex != 255 && remappedIndex < material.PixelShaderConstants.Count)
                {
                    return material.PixelShaderConstants[remappedIndex];
                }
            }

            return 0;
        }

        private static bool HasAlphaBlendingEnabled(Shaders.Shader shader)
        {
            if ((shader.UbershaderRequirementFlags & (1L << (int)SHADER_REQUIREMENTS.FORCE_TO_ALPHA)) != 0 ||
                (shader.UbershaderRequirementFlags & (1L << (int)SHADER_REQUIREMENTS.EARLY_ALPHA)) != 0 ||
                (shader.UbershaderRequirementFlags & (1L << (int)SHADER_REQUIREMENTS.POST_ALPHA)) != 0 ||
                (shader.UbershaderRequirementFlags & (1L << (int)SHADER_REQUIREMENTS.LOWRES_ALPHA)) != 0 ||
                (shader.UbershaderRequirementFlags & (1L << (int)SHADER_REQUIREMENTS.FORCE_TO_HI_ALPHA)) != 0)
            {
                return true;
            }

            int? useAlphaFeatureIndex = ShaderUtility.GetShaderFunctionalityIndex(shader.Ubershader, ShaderIndexType.FEATURES, "USE_ALPHA_AS_BLENDFACTOR");
            if (useAlphaFeatureIndex.HasValue)
            {
                if ((shader.UbershaderFeatureFlags & (1L << useAlphaFeatureIndex.Value)) != 0)
                    return true;
            }

            int? forceToAlphaFeatureIndex = ShaderUtility.GetShaderFunctionalityIndex(shader.Ubershader, ShaderIndexType.FEATURES, "FORCE_TO_ALPHA");
            if (forceToAlphaFeatureIndex.HasValue)
            {
                if ((shader.UbershaderFeatureFlags & (1L << forceToAlphaFeatureIndex.Value)) != 0)
                    return true;
            }

            return false;
        }

        private static System.Windows.Media.Color GetEmissiveTint(Materials.Material material)
        {
            int emissiveTint = -1;
            switch (material.Shader.Ubershader)
            {
                case SHADER_LIST.CA_ENVIRONMENT:
                    emissiveTint = (int)CA_ENVIRONMENT.PARAMETERS.EMISSIVE_TINT;
                    break;
                case SHADER_LIST.CA_CHARACTER:
                    emissiveTint = (int)CA_CHARACTER.PARAMETERS.EMISSIVE_TINT;
                    break;
                case SHADER_LIST.CA_LIGHTMAP_ENVIRONMENT:
                    emissiveTint = (int)CA_LIGHTMAP_ENVIRONMENT.PARAMETERS.EMISSIVE_TINT;
                    break;
            }

            if (emissiveTint != -1 && emissiveTint < material.Shader.PixelShaderParameterRemaps.Count)
            {
                int remappedIndex = material.Shader.PixelShaderParameterRemaps[emissiveTint];
                if (remappedIndex != 255 && remappedIndex < material.PixelShaderConstants.Count)
                {
                    UberShaderParameterType? parameterType = ShaderUtility.GetParameterType(material.Shader.Ubershader, "EMISSIVE_TINT");
                    if (parameterType.HasValue)
                    {
                        float r = 0, g = 0, b = 0, a = 1.0f;

                        switch (parameterType.Value)
                        {
                            case UberShaderParameterType.Float3:
                            case UberShaderParameterType.Half3:
                                if (remappedIndex < material.PixelShaderConstants.Count)
                                    r = material.PixelShaderConstants[remappedIndex];
                                if (remappedIndex + 1 < material.PixelShaderConstants.Count)
                                    g = material.PixelShaderConstants[remappedIndex + 1];
                                if (remappedIndex + 2 < material.PixelShaderConstants.Count)
                                    b = material.PixelShaderConstants[remappedIndex + 2];
                                break;
                            case UberShaderParameterType.Float4:
                            case UberShaderParameterType.Half4:
                                if (remappedIndex < material.PixelShaderConstants.Count)
                                    r = material.PixelShaderConstants[remappedIndex];
                                if (remappedIndex + 1 < material.PixelShaderConstants.Count)
                                    g = material.PixelShaderConstants[remappedIndex + 1];
                                if (remappedIndex + 2 < material.PixelShaderConstants.Count)
                                    b = material.PixelShaderConstants[remappedIndex + 2];
                                if (remappedIndex + 3 < material.PixelShaderConstants.Count)
                                    a = material.PixelShaderConstants[remappedIndex + 3];
                                break;
                        }

                        r = Math.Max(0, Math.Min(1, r));
                        g = Math.Max(0, Math.Min(1, g));
                        b = Math.Max(0, Math.Min(1, b));
                        a = Math.Max(0, Math.Min(1, a));

                        return System.Windows.Media.Color.FromArgb(
                            (byte)(a * 255),
                            (byte)(r * 255),
                            (byte)(g * 255),
                            (byte)(b * 255)
                        );
                    }
                }
            }

            return System.Windows.Media.Colors.Transparent;
        }
    }
}
