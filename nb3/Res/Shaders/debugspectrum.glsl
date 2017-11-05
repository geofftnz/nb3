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
	texcoord = vertex.xy;
}

//|common

#define PI 3.1415926535897932384626433832795
#define PIOVER2 1.5707963267948966192313216916398
#define LOGe10 2.30258509299

vec3 log10(vec3 s)
{
	return log(s.rgb) / LOGe10;
}

vec4 todB(vec4 s)
{
	// ignore 4th component (stereo angle)
	s.rgb = 20.0*log10(s.rgb);
	s.rgb = max(vec3(0.0),vec3(1.0) + (s.rgb / vec3(100.0)));
	return s;
}
float todB(float s)
{
	s = 20.0*log(s);
	s = max(0.0,1.0 + ((s+20.0) / 200.0));
	return s;
}


vec3 colscale(float s)
{
	vec3 col  = vec3(0.0,0.0,0.0   );

	vec3 col0 = vec3(0.0,0.0,0.05   );
	vec3 col1 = vec3(0.0,0.1,0.9 );
	vec3 col2 = vec3(0.0,0.6,0.6   );
	vec3 col3 = vec3(0.9,0.9,0.0  );
	vec3 col4 = vec3(0.9,0.0,0.0  );
	vec3 col5 = vec3(1.0,1.0,1.0);
	
	col = mix(col0,col1,clamp(s*2.5,0.0,1.0));
	col = mix(col,col2,clamp((s-0.3)*2.0,0.0,1.0));
	col = mix(col,col3,clamp((s-0.5)*4.0,0.0,1.0));
	col = mix(col,col4,clamp((s-0.6)*5.0,0.0,1.0));
	col = mix(col,col5,clamp((s-0.8)*6.0,0.0,1.0));

	return col;
}

vec3 colscale_bw(float s)
{
	return vec3(s*s*s*s*4.0);
}


//|spectrum_common
vec2 texel = vec2(1.0/1024.0,0.0);

vec4 getSample(sampler2D spectrum, vec2 t)
{
	vec2 raw = texture2D(spectrum,t).rg;

	// TODO: potentially don't need this
	float sep = asin((raw.g - raw.r) / (raw.g + raw.r)) / PIOVER2;

	vec4 s = vec4(raw.rg,(raw.r+raw.g)*0.5, sep);

	return s;
}

vec4 getSample_sharpen(sampler2D spectrum, vec2 t)
{
	vec2 raw = vec2(0.0);

	/*
	raw += texture2D(spectrum,t + texel*4.0).rg * -0.0625;
	raw += texture2D(spectrum,t + texel*3.0).rg * -0.125;
	raw += texture2D(spectrum,t + texel*2.0).rg * -0.25;
	raw += texture2D(spectrum,t + texel).rg * -0.5;
	raw += texture2D(spectrum,t).rg * 2.0;
	raw += texture2D(spectrum,t - texel).rg * -0.5;
	raw += texture2D(spectrum,t - texel*2.0).rg * -0.25;
	raw += texture2D(spectrum,t - texel*3.0).rg * -0.125;
	raw += texture2D(spectrum,t - texel*4.0).rg * -0.0625;
	*/

	raw = texture2D(spectrum,t).rg * 2.0;

	vec2 background = vec2(0.0);
	float ofs = 1.0;
	for(int i=0;i<8;i++){

		background += texture2D(spectrum,t - texel * ofs).rg / (ofs*0.7 + 1.0);
		background += texture2D(spectrum,t + texel * ofs).rg / (ofs*0.7 + 1.0);

		ofs += 1.0;
	}

	//background /= 10.0;
	raw -= background * 0.9;


	raw = max(vec2(0.0),raw);

	// TODO: potentially don't need this
	float sep = asin((raw.g - raw.r) / (raw.g + raw.r)) / PIOVER2;

	vec4 s = vec4(raw.rg,(raw.r+raw.g)*0.5, sep);

	return s;
}


vec4 scaleSpectrum(vec4 s)
{
	return todB(s);
}

float fscale(float x)
{
	return x * 0.1 + 0.9 * x * x;
}


//|waterfall_frag
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D spectrumTex;
uniform float currentPosition;
uniform float currentPositionEst;

#include "Common/gamma.glsl";
#include ".|common";
#include ".|spectrum_common";

vec4 renderSpectrum(vec2 t)
{
	t.x = fscale(t.x);

	// spectrum
	//vec3 col = colscale(scaleSpectrum(getSample(spectrumTex,t)).b);

	//vec4 samp = scaleSpectrum(getSample(spectrumTex,t));
	vec4 samp = scaleSpectrum(getSample_sharpen(spectrumTex,t));

	// normal
	vec3 col = colscale(samp.b);

	// stereo diff
	//vec3 col_l = colscale_bw(samp.r * 1.5 - samp.g * 0.5) * vec3(1.0,0.5,0.0);
	//vec3 col_r = colscale_bw(samp.g * 1.5 - samp.r * 0.5) * vec3(0.0,0.5,1.0);
	//vec3 col = (col_l + col_r);

	// stereo separation
	//float s = todB(getSample(spectrumTex,t)).a;
	//vec3 col = mix(vec3(1.0,0.8,0.0), vec3(0.0,0.2,1.0), s*0.5+0.5);


	// decoding position indicator
	float a = 1.0 - smoothstep(abs(currentPosition-t.y),0.0,0.5/1024.0);
	col += vec3(1.0,0.0,0.0) * a * 0.5;

	// actual position indicator
	float b = 1.0 - smoothstep(abs(currentPositionEst-t.y),0.0,0.5/1024.0);
	col += vec3(0.0,1.0,0.0) * b * 0.5;

	return vec4(col,1.0);
}


void main(void)
{
	vec2 t = texcoord.yx;

	vec4 col = renderSpectrum(t);
	
	// gamma
	col.rgb = l2g(col.rgb);

	out_Colour = col;	
}

//|waterfall2_frag
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D spectrumTex;
uniform float currentPosition;
uniform float currentPositionEst;

#include "Common/gamma.glsl";
#include ".|common";
#include ".|spectrum_common";

float scaleSpectrum2(float s)
{
	return 1.0 + (20.0 * (log(s) / LOGe10)) / 200.0;
}

vec4 renderSpectrum(vec2 t)
{
	t.x = t.x*t.x;

	// spectrum
	vec3 col = colscale(scaleSpectrum2(texture2D(spectrumTex,t).r));

	// stereo separation
	//float s = todB(getSample(spectrumTex,t)).a;
	//vec3 col = mix(vec3(1.0,0.0,0.0), vec3(0.0,0.0,1.0), s*0.5+0.5);

	float a = 1.0 - smoothstep(abs(currentPosition-t.y),0.0,0.5/1024.0);
	col += vec3(1.0,0.0,0.0) * a * 0.5;

	float b = 1.0 - smoothstep(abs(currentPositionEst-t.y),0.0,0.5/1024.0);
	col += vec3(0.0,1.0,0.0) * b * 0.5;

	return vec4(col,1.0);
}


void main(void)
{
	vec2 t = texcoord.yx;

	vec4 col = renderSpectrum(t);
	
	// gamma
	col.rgb = l2g(col.rgb);

	out_Colour = col;	
}


//|spectrum_frag
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D spectrumTex;
uniform float currentPosition;
uniform float currentPositionEst;

#include "Common/gamma.glsl";
#include ".|common";
#include ".|spectrum_common";


float plotPoint(float xpos, float xsample, float width)
{
	return 1.0 - smoothstep(0.0,width,abs(xpos - xsample));
}

vec4 renderGraph_multi(vec2 t)
{
	vec3 col = vec3(0.0);
	float ty = fscale(t.y);
	float tx;

	//t.y = floor(t.y * 512.0) / 512.0;
	ty = floor(ty * 1024.0) / 1024.0;

	for (float i = 0.0;i<1.0;i+=0.05)
	{
		tx = texel.x * i * 10.0;
		float s = scaleSpectrum(getSample(spectrumTex,vec2(ty,currentPositionEst - tx))).b; 

		col += colscale(s) * plotPoint(t.x,s,0.02) * 0.3;
	}

	return vec4(col,1.0);
}


vec4 renderGraph_minmax(vec2 t)
{
	vec3 col = vec3(0.0,0.0,0.0);
	float ty = fscale(t.y);

	float samplemax = 0.0, samplemin = 1000.0;
	for (float i = 0.0;i<1.0;i+=0.05)
	{
		float s = scaleSpectrum(getSample(spectrumTex,vec2(ty,currentPositionEst - texel.x * i * 50.0))).b; 
		samplemax = max(samplemax,s);
		samplemin = min(samplemin,s);
	}

	float sample0 = scaleSpectrum(getSample(spectrumTex,vec2(ty,currentPositionEst))).b;

	col += vec3(1.0) * plotPoint(t.x,samplemax,0.02) * 0.05;
	col += vec3(1.0) * plotPoint(t.x,samplemin,0.02) * 0.05;
	col += colscale(sample0) * plotPoint(t.x,sample0,0.08) * 2.0;
	return vec4(col,1.0);
}

vec4 renderGraph_original(vec2 t)
{
	vec3 col = vec3(0.0,0.0,0.0);
	float ty = fscale(t.y);

	for (float i = 0.0;i<1.0;i+=0.1)
	{
		float s = scaleSpectrum(getSample(spectrumTex,vec2(ty,currentPositionEst - texel.x * i * 4.0))).b;

		float a = abs(s-(t.x+i*0.05));
        float w = 0.05 - smoothstep(0.0,1.0,i) * 0.03;

        a = 1.0 - smoothstep(0.0,w,a);
		//a = 1.0 - step(w,a);
		col += colscale(s) * a * 0.7;// * (1.0 - smoothstep(0.03,0.9,i) * 0.9);
	}

	return vec4(col,1.0);
}


void main(void)
{
	vec2 t = texcoord.yx;

	vec4 col = renderGraph_multi(t);
	//vec4 col = renderGraph_minmax(t);
	
	// gamma
	col.rgb = l2g(col.rgb);

	out_Colour = col;	
}

