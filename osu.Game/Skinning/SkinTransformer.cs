// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A default skin transformer, which falls back to the provided skin by default.
    /// </summary>
    /// <remarks>
    /// Implementations of skin transformers should generally derive this class and override
    /// individual lookup methods, modifying the lookup flow as required.
    /// </remarks>
    public abstract class SkinTransformer : ISkinTransformer
    {
        public ISkin Skin { get; }

        protected SkinTransformer(ISkin skin)
        {
            Skin = skin ?? throw new ArgumentNullException(nameof(skin));
        }

        public virtual Drawable? GetDrawableComponent(ISkinComponentLookup lookup) => Skin.GetDrawableComponent(lookup);

        public virtual Texture? GetTexture(string componentName, Vector2? maxSize = null, WrapMode wrapModeS = default, WrapMode wrapModeT = default) => Skin.GetTexture(componentName, maxSize, wrapModeS, wrapModeT);

        public virtual ISample? GetSample(ISampleInfo sampleInfo) => Skin.GetSample(sampleInfo);

        public virtual IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup) where TLookup : notnull where TValue : notnull => Skin.GetConfig<TLookup, TValue>(lookup);
    }
}
