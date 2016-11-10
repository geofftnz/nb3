//|effect
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 1) in vec2 pos;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D spectrumTex;
uniform sampler2D spectrum2Tex;
uniform sampler2D audioDataTex;
uniform float currentPosition;
uniform float currentPositionEst;

#include "Common/gamma.glsl";

#define DATARES 256

float stexel = 1.0/DATARES;
float ttexel = 1.0/1024.0;

float getDataSample(float index, float offset)
{
	vec2 t = vec2(index * stexel, offset * ttexel + currentPositionEst);
	
	float s = texture2D(audioDataTex,t).r;
	return s;
}


void main(void)
{
	vec3 col = vec3(pos.xy,0.2);

	float r = dot(pos,pos);

	float time_since_pulse = 1.0 - getDataSample(0.0,0.0);

	float pulse = 1.0 / exp(time_since_pulse * 4.0);
	float pulse2 = 1.0 / exp(time_since_pulse * 2.0);


	col.rgb = mix(vec3(0.0),vec3(1.0,1.2,1.5) * pulse2,1.0-smoothstep(pulse,pulse + 0.02,r));

	
	// gamma
	col.rgb = l2g(col.rgb);

	out_Colour = vec4(col,1.0);	
}

