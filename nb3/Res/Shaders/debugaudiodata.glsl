// line graphs below spectrum.
//|vert
#version 410
precision highp float;
layout (location = 0) in vec3 vertex;
layout (location = 0) out vec2 texcoord;
uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;

void main() 
{
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vertex.xy,0.0,1.0);
	texcoord = vertex.xy * vec2(0.5) + vec2(0.5);
}


//|frag
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D audioDataTex;
uniform float currentPosition;
uniform float currentPositionEst;

#include "Common/gamma.glsl";
#include "debugspectrum.glsl|common";

#define DATARES 256

float stexel = 1.0/DATARES;
float ttexel = 1.0/1024.0;


float getSample(sampler2D tex, vec2 t)
{
	float s = texture2D(tex,t).r;
	//s = todB(s);
	return s;
}


void main(void)
{
	vec2 t = texcoord.yx;
	//t.x = 1.0 - t.x;  // invert y

	float index = (floor(t.x * DATARES) / DATARES) + stexel * 0.5;
	float offset = 1.0 - fract(t.x * DATARES);

	float s = getSample(audioDataTex,vec2(index,t.y));

	//s *= 10.0;
	

	float a = 1.0 - smoothstep(0.0,0.05,abs(s - offset));

	vec3 col = colscale(s) * a * 2.0;

	float fade = smoothstep(mod(t.y-currentPositionEst+1.0,1.0)*128.0,0.0,1.0);
	float fade2 = 1.0 - smoothstep(mod(currentPositionEst-t.y+1.0,1.0)*256.0,0.0,1.0);
	col *= fade;
	col *= (1.0+fade2*2.0);

	// gamma
	col.rgb = l2g(col.rgb);

	out_Colour = vec4(col,1.0);	
}

