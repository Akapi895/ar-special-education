import LandingNavbar from '../../features/landing/LandingNavbar';
import HeroSection from '../../features/landing/HeroSection';
import AboutSection from '../../features/landing/AboutSection';
import CPASection from '../../features/landing/CPASection';
import FeaturesSection from '../../features/landing/FeaturesSection';
import HowItWorksSection from '../../features/landing/HowItWorksSection';
import CTASection from '../../features/landing/CTASection';
import LandingFooter from '../../features/landing/LandingFooter';

const LandingPage = () => {
  return (
    <div className="min-h-screen bg-white">
      <LandingNavbar />
      <HeroSection />
      <AboutSection />
      <CPASection />
      <FeaturesSection />
      <HowItWorksSection />
      <CTASection />
      <LandingFooter />
    </div>
  );
};

export default LandingPage;
