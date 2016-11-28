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

float stexel = 1.0/DATARES;
float ttexel = 1.0/1024.0;

float getDataSample(float index, float offset)
{
	vec2 t = vec2(index * stexel, currentPositionEst - offset * ttexel);
	
	float s = texture2D(audioDataTex,t).r;
	return s;
}

vec2 texel = vec2(1.0/1024.0,0.0);

vec4 getSample(sampler2D spectrum, vec2 t)
{
	vec2 raw = texture2D(spectrum,t).rg;

	float sep = asin((raw.g - raw.r) / (raw.g + raw.r)) / PIOVER2;

	vec4 s = vec4(raw.rg,(raw.r+raw.g)*0.5, sep);

	return s;
}

vec4 getOffsetSample(sampler2D spectrum, float freq, float offset)
{
	return getSample(spectrum, vec2(freq, currentPositionEst - offset * ttexel));
}


vec4 scaleSpectrum(vec4 s)
{
	return todB(s);
}

float fscale(float x)
{
	return x * 0.1 + 0.9 * x * x;
}


// sub-effects (split to common?)

// bubbles that track the peak-detector
vec4 FreqTrackerBubbles(vec2 texcoord, float interval)
{
	vec3 col = vec3(0.0);

	for (float x = 0.0; x <= 1.01; x+= interval)
	{
		if (x < texcoord.x - 0.1 || x > texcoord.x + 0.1)
			continue;

		// draw a circle (x,y,r)
		vec3 circle = vec3(x,0.1,0.02);
		vec3 ccol = vec3(0.0,0.1,1.0);

		float time = (1.0-x) * 100.0;

		float freq = getDataSample(0.0,time);
		circle.y = 0.1 + 0.8 * sqrt(freq);
		circle.z = 0.0001 + 0.3 * max(0.0,getDataSample(3.0,time)-0.3) ;
		ccol = colscale(3.0 * max(0.0,getDataSample(3.0,time) - 0.2));

		// get distance from centre
		float d = length(texcoord.xy-circle.xy) - circle.z;
	
		float cinside = 1.0;//smoothstep(-circle.z,0.0,d);		
		col += ccol * (1.0-smoothstep(0.0,0.01,d)) * cinside * cinside;
	}

	return vec4(col,1.0);
}

// a large pulse of light from the background
// pos should be zero-centred
float PulseBackground(vec2 pos, float data_index, float exp_falloff, float ss1, float ss2)
{
	float time_since_pulse = 1.0 - getDataSample(data_index,0.0);

	return exp(-time_since_pulse * exp_falloff) * (1.0 - smoothstep(ss1,ss2,length(pos)));
}

// quantize into grid
vec4 GridQuantize(vec2 pos, vec2 size)
{
	vec2 pos2 = pos * size;

	pos2.x += sin(pos.y * 32.0) * 0.1;
	pos2.y += sin(pos.x * 32.0) * 0.1;

	float ofs = mod(floor(pos2.y),2.0);
	pos2.x += ofs*0.5;

	return vec4(floor(pos2.x),floor(pos2.y), fract(pos2.x),fract(pos2.y));
}

vec4 GridQuantizeRegular(vec2 pos, vec2 size)
{
	vec2 pos2 = pos * size;
	return vec4(floor(pos2.x),floor(pos2.y), fract(pos2.x),fract(pos2.y));
}


// a grid of spots
vec4 SpotGrid(vec2 pos, vec2 size)
{
	vec4 col = vec4(0.05);

	// determine spot coordinates
	vec4 gpos = GridQuantize(pos,size);

	gpos.xy /= size;

	gpos.y *= 2.0;
	if (gpos.y>1.0) gpos.y = 2.0 - gpos.y;

	// sample spectrum
	float spectrum = scaleSpectrum ( getOffsetSample ( spectrumTex, (1.0-gpos.y), (1.0-gpos.x) * 40.0)).b;
	//col.rgb = colscale(spectrum);
	col.rgb = vec3(0.0,0.0,0.4) * 0.5;
	
	float r = length(gpos.zw * 2.0 - 1.0);
	float r2 = spectrum * 1.5;
	float c = 1.0 - smoothstep(r2,r2 * 1.05,r);
	
	col.a = c;

	return col;
}


void main(void)
{
	vec3 col = vec3(0.0); //vec3(texcoord.xy,0.2);
	//vec3 col = vec3(texcoord.xy,0.2);

	vec3 pulse = vec3(0.0);
	pulse += vec3(0.0,0.05,0.6) * PulseBackground(pos,6.0,4.0,0.6,1.3);
	pulse += vec3(0.0,0.4,0.7) * PulseBackground(pos,6.0,6.0,0.5,1.3);
	pulse += vec3(0.0,0.3,0.1) * PulseBackground(pos,6.0,8.0,0.4,1.3);
	pulse += vec3(1.0,1.0,1.0) * PulseBackground(pos,6.0,10.0,0.3,1.0);

	col.rgb += pulse;	

	vec4 spotgrid = SpotGrid(texcoord,vec2(60.0,40.0));
	//col.rgb = mix(col.rgb,spotgrid.rgb,spotgrid.a);
	col.rgb += spotgrid.rgb * spotgrid.a;


	col += FreqTrackerBubbles(texcoord,0.08).rgb;

	// vignette
	col.rgb *= 1.0 - smoothstep(1.1,1.4,length(pos));
	
	// gamma
	col.rgb = l2g(col.rgb);

	out_Colour = vec4(col,1.0);	
}

