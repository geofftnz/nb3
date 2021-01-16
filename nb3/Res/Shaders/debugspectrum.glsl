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
#define DATARES 256

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

	raw = texture2D(spectrum,t).rg * 2.0;

	vec2 background = vec2(0.0);
	float ofs = 1.0;
	for(int i=0;i<16;i++){

		background += texture2D(spectrum,t - texel * ofs).rg / (ofs*0.2 + 1.0);
		background += texture2D(spectrum,t + texel * ofs).rg / (ofs*0.2 + 1.0);

		ofs += 1.0;
	}

	raw -= background * 0.2;


	raw = max(vec2(0.0),raw);

	// TODO: potentially don't need this
	float sep = asin((raw.g - raw.r) / (raw.g + raw.r)) / PIOVER2;

	vec4 s = vec4(raw.rg,(raw.r+raw.g)*0.5, sep);

	return s;
}

vec4 getSample_sharpen_in_time(sampler2D spectrum, vec2 t)
{
	vec2 raw = vec2(0.0);

	raw = texture2D(spectrum,t).rg * 2.0;

	vec2 background = vec2(0.0);
	float ofs = 1.0;
	for(int i=0;i<8;i++){
		background += texture2D(spectrum,t - texel.yx * ofs).rg;
		ofs += 1.0;
	}

	raw -= background * 0.5;

	raw = max(vec2(0.0),raw);

	// TODO: potentially don't need this
	float sep = asin((raw.g - raw.r) / (raw.g + raw.r)) / PIOVER2;

	vec4 s = vec4(raw.rg,(raw.r+raw.g)*0.5, sep);

	return s;
}

vec4 getSample_sharpen_in_time_smooth_in_freq(sampler2D spectrum, vec2 t)
{
	vec2 raw = vec2(0.0);

	//raw = texture2D(spectrum,t).rg * 2.0;
	raw += texture2D(spectrum,t - texel.xy * vec2(-2.0,0.0)).rg * 0.25;
	raw += texture2D(spectrum,t - texel.xy * vec2(-1.0,0.0)).rg * 0.5;
	raw += texture2D(spectrum,t - texel.xy * vec2(0.0, 0.0)).rg;
	raw += texture2D(spectrum,t - texel.xy * vec2(1.0, 0.0)).rg * 0.5;
	raw += texture2D(spectrum,t - texel.xy * vec2(2.0, 0.0)).rg * 0.25;
	//raw *= 2.0;

	vec2 background = vec2(0.0);
	float ofs = -1.0;
	for(int i=0;i<8;i++){
		background *= 0.8;
		background += texture2D(spectrum,t + texel.xx * vec2(-2.0,ofs)).rg * 0.25;
		background += texture2D(spectrum,t + texel.xx * vec2(-1.0,ofs)).rg * 0.5;
		background += texture2D(spectrum,t + texel.xx * vec2(0.0,ofs)).rg;
		background += texture2D(spectrum,t + texel.xx * vec2(1.0,ofs)).rg * 0.5;
		background += texture2D(spectrum,t + texel.xx * vec2(2.0,ofs)).rg * 0.25;
		ofs -= 1.0;
	}

	raw -= background * 0.5;

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

float fscale_inv(float y)
{
	return (sqrt(360.*y+1.)-1)/18.;
}

float getAudioDataSample(sampler2D tex, float index, float time)
{
	return texture2D(tex,vec2((index+0.5)/DATARES,time)).r;
}


//|waterfall_frag
// Main spectrum display
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D spectrumTex;
uniform sampler2D audioDataTex;
uniform float currentPosition;
uniform float currentPositionEst;

#include "Common/gamma.glsl";
#include ".|common";
#include ".|spectrum_common";

vec4 renderSpectrum(vec2 t)
{
	float original_tx = t.x;
	t.x = fscale(t.x);

	// spectrum
	//vec3 col = colscale(scaleSpectrum(getSample(spectrumTex,t)).b);

	vec4 samp = scaleSpectrum(getSample(spectrumTex,t));
	//vec4 samp = scaleSpectrum(getSample_sharpen(spectrumTex,t));
	//vec4 samp = scaleSpectrum(getSample_sharpen_in_time(spectrumTex,t));

	float sval = samp.b;
	// fade out old
	float fade = smoothstep(mod(t.y-currentPositionEst+1.0,1.0)*128.0,0.0,1.0);
	sval *= fade;

	// normal
	vec3 col = colscale(sval);

	float fade2 = 1.0 - smoothstep(mod(currentPositionEst-t.y+1.0,1.0)*256.0,0.0,1.0);
	col *= (1.0+fade2*2.0);
	
	// stereo diff
	//vec3 col_l = colscale_bw(samp.r * 1.5 - samp.g * 0.5) * vec3(1.0,0.5,0.0);
	//vec3 col_r = colscale_bw(samp.g * 1.5 - samp.r * 0.5) * vec3(0.0,0.5,1.0);
	//vec3 col = (col_l + col_r);

	// stereo separation
	//float s = todB(getSample(spectrumTex,t)).a;
	//col = mix(vec3(1.0,0.8,0.0), vec3(0.0,0.2,1.0), s*0.5+0.5);


	// decoding position indicator
	//float a = 1.0 - smoothstep(abs(currentPosition-t.y),0.0,0.5/1024.0);
	//col += vec3(1.0,0.0,0.0) * a * 0.5;

	// actual position indicator
	//float b = 1.0 - smoothstep(abs(currentPositionEst-t.y),0.0,0.5/1024.0);
	//col += vec3(0.0,1.0,0.0) * b * 0.5;

	// marker for dominant frequency detector
	float df = getAudioDataSample(audioDataTex,0.0,t.y);
	float da = getAudioDataSample(audioDataTex,2.0,t.y);
	//df = fscale_inv(df);
	//df = fscale(df);
	//df *= df;
	//col.g += (1.0 - clamp(abs(df - t.y) * 300.0,0.0,1.0))*da*da ;
	//float a = 1.0 - step(0.001,abs(original_tx - df));
	float a = 1.0 - smoothstep(abs(t.x - df),0.0,0.001);  
	//col.r += a * fade;
	//col.g += a*da*da * fade;
	col.g += a*da*da*fade*3.;



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
// Smaller spectrum
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
	//t.x = fscale(t.x);

	// spectrum
	//vec3 col = colscale(scaleSpectrum2(texture2D(spectrumTex,t).r));
	vec4 samp = scaleSpectrum(getSample_sharpen_in_time_smooth_in_freq(spectrumTex,t));
	//vec4 samp = scaleSpectrum(getSample(spectrumTex,t));

	float sval = samp.b;
	// fade out old
	float fade = smoothstep(mod(t.y-currentPositionEst+1.0,1.0)*128.0,0.0,1.0);
	sval *= fade;

	// normal
	vec3 col = colscale(sval);

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
// Spectrum display (current frame, bar graph)
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D spectrumTex;
uniform sampler2D audioDataTex;
uniform float currentPosition;
uniform float currentPositionEst;

#include "Common/gamma.glsl";
#include ".|common";
#include ".|spectrum_common";

float stexel = 1.0/DATARES;
float ttexel = 1.0/1024.0;

float plotPoint(float xpos, float xsample, float width)
{
	return 1.0 - smoothstep(0.0,width,abs(xpos - xsample));
}

float getCurrentAudioDataSample(float index)
{
	float s = texture2D(audioDataTex,vec2((index+0.5)/DATARES,currentPositionEst)).r;
	return s;
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

	// marker for dominant frequency detector
	
	float df = getCurrentAudioDataSample(0.0);
	float da = getCurrentAudioDataSample(2.0);
	//df = fscale(df);
	//df *= df;
	col.g += (1.0 - clamp(abs(df - ty) * 300.0,0.0,1.0))*da*da ;

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

