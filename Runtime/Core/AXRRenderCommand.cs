﻿/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace onAirXR.Client {
    public abstract class AXRRenderCommand {
        public abstract void Issue(IntPtr renderFuncPtr, int arg);
        public abstract void Issue(IntPtr renderFuncPtr, int arg, IntPtr data);
        public abstract void SetRenderTarget(RenderTexture renderTarget);
        public abstract void Clear();
    }

    public class AXRCameraEventRenderCommand : AXRRenderCommand {
        private CommandBuffer _commandBuffer;

        public AXRCameraEventRenderCommand(Camera camera, CameraEvent evt) {
            _commandBuffer = new CommandBuffer();
            camera.AddCommandBuffer(evt, _commandBuffer);
        }

        public override void Issue(IntPtr renderFuncPtr, int arg) {
            _commandBuffer.IssuePluginEvent(renderFuncPtr, arg);
        }

        public override void Issue(IntPtr renderFuncPtr, int arg, IntPtr data) {
            _commandBuffer.IssuePluginEventAndData(renderFuncPtr, arg, data);
        }

        public override void SetRenderTarget(RenderTexture renderTarget) {
            _commandBuffer.SetRenderTarget(renderTarget);
        }

        public override void Clear() {
            _commandBuffer.Clear();
        }
    }

    public class AXRImmediateRenderCommand : AXRRenderCommand {
        public override void Issue(IntPtr renderFuncPtr, int arg) {
            GL.IssuePluginEvent(renderFuncPtr, arg);
        }

        public override void Issue(IntPtr renderFuncPtr, int arg, IntPtr data) {
            throw new NotImplementedException();
        }

        public override void SetRenderTarget(RenderTexture renderTarget) {
            throw new NotImplementedException();
        }

        public override void Clear() { }
    }
}
