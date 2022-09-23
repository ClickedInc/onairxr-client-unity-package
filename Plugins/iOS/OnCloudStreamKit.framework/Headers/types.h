#ifndef _OCS_TYPES_H_
#define _OCS_TYPES_H_

typedef struct _OCS_VECTOR2D
{
    float x;
    float y;
} OCS_VECTOR2D;

typedef struct _OCS_VECTOR3D
{
    float x;
    float y;
    float z;
} OCS_VECTOR3D;

typedef struct _OCS_VECTOR4D
{
    float x;
    float y;
    float z;
    float w;
} OCS_VECTOR4D;

typedef struct OCSUnityPluginMessage {
    void *source;
    const void *data;
    int length;
} OCSPluginMessage;

typedef struct {
    uint8_t* buffer;
    int size;
} StringBuffer;


#endif //_OCS_TYPES_H_
