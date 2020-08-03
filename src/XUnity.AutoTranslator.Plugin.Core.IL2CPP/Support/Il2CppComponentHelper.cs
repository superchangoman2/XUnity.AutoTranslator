﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.IL2CPP.Text;
using XUnity.Common.Constants;
using XUnity.Common.IL2CPP.Extensions;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Support
{
   internal class Il2CppComponentHelper : IComponentHelper
   {
      private static GameObject[] _objects = new GameObject[ 128 ];
      private static readonly string XuaIgnore = "XUAIGNORE";

      public string GetText( object ui )
      {
         return ( ui as ITextComponent )?.Text;
      }

      public bool IsComponentActive( object ui )
      {
         return ( ( ui as ITextComponent )?.Component ).gameObject?.activeInHierarchy ?? false;
      }

      public bool IsKnownTextType( object ui )
      {
         return ui is ITextComponent;
      }

      public bool IsNGUI( object ui )
      {
         return false;
      }

      public bool IsSpammingComponent( object ui )
      {
         return ui is ITextComponent tc && tc.IsSpammingComponent();
      }

      public void SetText( object ui, string text )
      {
         if( ui is ITextComponent tc )
         {
            tc.Text = text;
         }
      }

      public bool ShouldTranslateTextComponent( object ui, bool ignoreComponentState )
      {
         if( ui is ITextComponent tc && tc.Component != null )
         {
            var component = tc.Component;

            // dummy check
            var go = component.gameObject;
            var ignore = go.HasIgnoredName();
            if( ignore )
            {
               return false;
            }

            if( !ignoreComponentState )
            {
               var behaviour = component.TryCastTo<Behaviour>();
               if( !go.activeInHierarchy || behaviour?.enabled == false ) // legacy "isActiveAndEnabled"
               {
                  return false;
               }
            }

            return !tc.IsPlaceholder();
         }

         return true;
      }

      public bool SupportsLineParser( object ui )
      {
         return Settings.GameLogTextPaths.Count > 0 && Settings.GameLogTextPaths.Contains( ( ( ui as ITextComponent )?.Component ).gameObject.GetPath() );
      }

      public bool SupportsRichText( object ui )
      {
         return ui is ITextComponent tc && tc.SupportsRichText();
      }

      public bool SupportsStabilization( object ui )
      {
         if( ui == null ) return false;

         return true;
      }

      public TextTranslationInfo GetOrCreateTextTranslationInfo( object ui )
      {
         var info = ui.GetOrCreateExtensionData<Il2CppTextTranslationInfo>();
         info.Initialize( ui );

         return info;
      }

      public TextTranslationInfo GetTextTranslationInfo( object ui )
      {
         var info = ui.GetExtensionData<Il2CppTextTranslationInfo>();

         return info;
      }

      public object CreateWrapperTextComponentIfRequiredAndPossible( object ui )
      {
         if( ui is Component tc )
         {
            var type = tc.GetIl2CppTypeSafe();

            if( Settings.EnableUGUI && UnityTypes.Text != null && UnityTypes.Text.Il2CppType.IsAssignableFrom( type ) )
            {
               return new TextComponent( tc );
            }
            else if( Settings.EnableTextMesh && UnityTypes.TextMesh != null && UnityTypes.TextMesh.Il2CppType.IsAssignableFrom( type ) )
            {
               return new TextMeshComponent( tc );
            }
            else if( Settings.EnableTextMeshPro && UnityTypes.TMP_Text != null && UnityTypes.TMP_Text.Il2CppType.IsAssignableFrom( type ) )
            {
               return new TMP_TextComponent( tc );
            }
         }
         return null;
      }

      public IEnumerable<object> GetAllTextComponentsInChildren( object go )
      {
         yield break;
      }

      public string[] GetPathSegments( object obj )
      {
         if( obj is GameObject go )
         {

         }
         else if( obj is ITextComponent tc )
         {
            go = tc.Component.gameObject;
         }
         else if( obj is Component comp )
         {
            go = comp.gameObject;
         }
         else
         {
            throw new ArgumentException( "Expected object to be a GameObject or component.", "obj" );
         }

         int i = 0;
         int j = 0;

         _objects[ i++ ] = go;
         while( go.transform.parent != null )
         {
            go = go.transform.parent.gameObject;
            _objects[ i++ ] = go;
         }

         var result = new string[ i ];
         while( --i >= 0 )
         {
            result[ j++ ] = _objects[ i ].name;
            _objects[ i ] = null;
         }

         return result;
      }

      public string GetPath( object obj )
      {
         if( obj is GameObject go )
         {

         }
         else if( obj is ITextComponent tc )
         {
            go = tc.Component.gameObject;
         }
         else if( obj is Component comp )
         {
            go = comp.gameObject;
         }
         else
         {
            throw new ArgumentException( "Expected object to be a GameObject or component.", "obj" );
         }

         StringBuilder path = new StringBuilder();
         var segments = GetPathSegments( go );
         for( int i = 0; i < segments.Length; i++ )
         {
            path.Append( "/" ).Append( segments[ i ] );
         }

         return path.ToString();
      }

      public bool HasIgnoredName( object obj )
      {
         if( obj is GameObject go )
         {

         }
         else if( obj is ITextComponent tc )
         {
            go = tc.Component.gameObject;
         }
         else if( obj is Component comp )
         {
            go = comp.gameObject;
         }
         else
         {
            throw new ArgumentException( "Expected object to be a GameObject or component.", "obj" );
         }

         return go.name.Contains( XuaIgnore );
      }

      public string GetTextureName( object texture, string fallbackName )
      {
         if( texture is Texture2D texture2d )
         {
            var name = texture2d.name;
            if( !string.IsNullOrEmpty( name ) )
            {
               return name;
            }
         }
         return fallbackName;
      }

      public void LoadImageEx( object texture, byte[] data, ImageFormat dataType, object originalTexture )
      {
         //Texture2D texture2D = (Texture2D)texture;
         //var format = texture2D.format;


         //using( var stream = new MemoryStream( data ) )
         //using( var binaryReader = new BinaryReader( stream ) )
         //{
         //   binaryReader.BaseStream.Seek( 12L, SeekOrigin.Begin );
         //   short num1 = binaryReader.ReadInt16();
         //   short num2 = binaryReader.ReadInt16();
         //   int num3 = (int)binaryReader.ReadByte();
         //   binaryReader.BaseStream.Seek( 1L, SeekOrigin.Current );
         //   Color[] colors = new Color[ (int)num1 * (int)num2 ];
         //   if( num3 == 32 )
         //   {
         //      for( int index = 0; index < (int)num1 * (int)num2; ++index )
         //      {
         //         float b = binaryReader.ReadByte() / 255f;
         //         float g = binaryReader.ReadByte() / 255f;
         //         float r = binaryReader.ReadByte() / 255f;
         //         float a = binaryReader.ReadByte() / 255f;
         //         colors[ index ] = new Color( r, g, b, a );
         //      }
         //   }
         //   else
         //   {
         //      for( int index = 0; index < (int)num1 * (int)num2; ++index )
         //      {
         //         float b = binaryReader.ReadByte() / 255f;
         //         float g = binaryReader.ReadByte() / 255f;
         //         float r = binaryReader.ReadByte() / 255f;
         //         colors[ index ] = new Color( r, g, b, 1f );
         //      }
         //   }
         //   texture2D.SetPixels( colors );
         //   texture2D.Apply();
         //}

         //if( originalTexture is Texture2D originalTexture2D )
         //{
         //   texture2D.name = originalTexture2D.name;
         //   texture2D.filterMode = originalTexture2D.filterMode;
         //   texture2D.wrapMode = originalTexture2D.wrapMode;
         //}

         // why no Image Conversion?
         throw new NotImplementedException();
      }

      public TextureDataResult GetTextureData( object texture )
      {
         // why no Image Conversion?
         throw new NotImplementedException();
      }

      public bool IsKnownImageType( object ui )
      {
         return false;
      }

      public object GetTexture( object ui )
      {
         return null;
      }

      public void SetTexture( object ui, object texture )
      {
         
      }

      public void SetAllDirtyEx( object ui )
      {
      }

      public object CreateEmptyTexture2D( int originalTextureFormat )
      {
         var format = (TextureFormat)originalTextureFormat;

         TextureFormat newFormat;
         switch( format )
         {
            case TextureFormat.RGB24:
               newFormat = TextureFormat.RGB24;
               break;
            case TextureFormat.DXT1:
               newFormat = TextureFormat.RGB24;
               break;
            case TextureFormat.DXT5:
               newFormat = TextureFormat.ARGB32;
               break;
            default:
               newFormat = TextureFormat.ARGB32;
               break;
         }

         return new Texture2D( 2, 2, newFormat, false );
      }
   }
}
